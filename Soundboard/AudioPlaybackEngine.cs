using System.Collections.Concurrent;
using System.Runtime;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Timer = System.Threading.Timer;

namespace HASCore.Soundboard;

/// Description
/// <summary>
///     Provides audio playback with caching, memory management, and idle cleanup.
/// </summary>
/// <remarks>
///     This engine uses NAudio to mix and play audio through a selected output device.
///     It caches sounds in memory up to a configurable limit, evicting the least
///     recently used sounds when the cache is full. The cache is also cleared after
///     a period of inactivity to free memory. The engine is thread‑safe and should
///     be accessed via the <see cref="Instance"/> singleton.
/// </remarks>
internal class AudioPlaybackEngine : IDisposable
{
    #region Singleton Instance

    /// Description
    /// <summary>
    ///     Gets the singleton instance of the audio playback engine.
    /// </summary>
    public static readonly AudioPlaybackEngine Instance = new(44100, 2);

    #endregion

    #region Constants

    /// Description
    /// <summary>
    ///     Overhead for the cached sound object in bytes – approximate overhead per cache entry.
    /// </summary>
    private const Int64 ObjectOverhead = 200;
    /// Description
    /// <summary>
    ///     Maximum cache size in bytes, currently 100 MB.
    /// </summary>
    private const Int64 MaxCacheSizeInBytes = 100L * 1024 * 1024;
    /// Description
    /// <summary>
    ///     Force GC after evicting this many items
    /// </summary>
    private const Int32 GCThreshold = 20;                      
    private static readonly TimeSpan IdleTimeout = TimeSpan.FromMinutes(60);

    #endregion

    #region Private Fields

    // NAudio playback components
    private WaveOutEvent? OutputDevice;
    private readonly MixingSampleProvider Mixer;

    // Cache storage with LRU tracking
    private readonly ConcurrentDictionary<String, CacheEntry> Cache = new();
    private Int64 TotalCacheSizeInBytes = 0;
    private readonly LinkedList<String> LRUList = new(); // Head = oldest, Tail = newest
    private readonly Lock CacheLock = new();

    // Idle cleanup
    private DateTime LastPlayTime = DateTime.Now;
    private readonly Timer? IdleTimer;

    // Garbage collection control
    private Int32 ItemsEvictedSinceLastGC = 0;

    #endregion

    #region Events

    /// <summary>
    ///     Occurs when all currently playing sounds have finished.
    /// </summary>
    public event EventHandler? AllInputEnded;

    #endregion

    #region Nested Types

    /// Description
    /// <summary>
    ///     Represents a cached sound with its memory footprint.
    /// </summary>
    private class CacheEntry(CachedSound sound)
    {
        public CachedSound Sound { get; } = sound;

        /// <summary>
        ///     Total size in bytes (audio data + object overhead).
        /// </summary>
        public Int64 SizeInBytes { get; } = sound.AudioData.Length * sizeof(Single) + ObjectOverhead;
    }

    #endregion

    #region Constructor

    /// Description
    /// <summary>
    ///     Initializes a new instance of the <see cref="AudioPlaybackEngine"/> class.
    /// </summary>
    /// <param name="sampleRate">The sample rate in Hz (default: 44100).</param>
    /// <param name="channelCount">The number of channels (default: 2 for stereo).</param>
    public AudioPlaybackEngine(Int32 sampleRate = 44100, Int32 channelCount = 2)
    {
        Mixer = new(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount))
        {
            ReadFully = true
        };
        Mixer.MixerInputEnded += OnMixerInputEnded;

        // Start a timer that checks every minute for idle cleanup.
        IdleTimer = new Timer(
            _ => CheckIdleAndClearCache(),
            null,
            TimeSpan.FromSeconds(60),
            TimeSpan.FromSeconds(60)
        );
    }

    #endregion

    #region Private Methods

    /// Description
    /// <summary>
    ///     Clears the entire cache if the engine has been idle for longer than <see cref="IdleTimeout"/>.
    /// </summary>
    private void CheckIdleAndClearCache()
    {
        if (DateTime.Now - LastPlayTime > IdleTimeout)
        {
            lock (CacheLock)
            {
                Cache.Clear();
                LRUList.Clear();
                TotalCacheSizeInBytes = 0;
                ItemsEvictedSinceLastGC = 0;
            }
            ForceGarbageCollection();
        }
    }

    /// Description
    /// <summary>
    ///     Forces garbage collection and Large Object Heap (LOH) compaction to reclaim memory.
    /// </summary>
    /// <remarks>
    ///     Called after evicting a batch of items or when the cache is cleared.
    ///     Uses <see cref="GCLargeObjectHeapCompactionMode.CompactOnce"/> to compact the LOH
    ///     where audio buffers (large <c>float[]</c> arrays) are stored.
    /// </remarks>
    private static void ForceGarbageCollection()
    {
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.Default;
    }

    /// Description
    /// <summary>
    ///     Evicts the least recently used item from the cache, optionally protecting a key.
    /// </summary>
    /// <param name="protectedKey">
    ///     A key that should not be evicted (e.g., the key just accessed).
    ///     Pass <c>null</c> to allow evicting any item.
    /// </param>
    /// <returns><c>true</c> if an item was evicted; otherwise, <c>false</c>.</returns>
    private Boolean EvictLeastRecentlyUsed(String? protectedKey = null)
    {
        lock (CacheLock)
        {
            if (LRUList.Count == 0)
                return false;

            // Find the oldest item that is not protected.
            LinkedListNode<String>? node = LRUList.First;
            while (node != null && protectedKey != null && node.Value == protectedKey)
            {
                node = node.Next;
            }

            if (node == null)
                return false; // All items are protected (shouldn't happen).

            String oldestKey = node.Value;
            if (Cache.TryRemove(oldestKey, out CacheEntry? removed))
            {
                TotalCacheSizeInBytes -= removed.SizeInBytes;
                LRUList.Remove(node);
                ItemsEvictedSinceLastGC++;

                // Trigger GC after reaching the threshold.
                if (ItemsEvictedSinceLastGC >= GCThreshold)
                {
                    ForceGarbageCollection();
                    ItemsEvictedSinceLastGC = 0;
                }
                return true;
            }
            else
            {
                // In case of inconsistency, remove the node from the list.
                LRUList.Remove(node);
                return false;
            }
        }
    }

    /// Description
    /// <summary>
    ///     Ensures the cache has enough space for a new sound, evicting LRU items as needed.
    /// </summary>
    /// <param name="soundSize">The size of the sound to be added, in bytes.</param>
    private void EnsureCacheSpace(Int64 soundSize)
    {
        lock (CacheLock)
        {
            // If the sound is larger than the entire cache, clear everything.
            if (soundSize > MaxCacheSizeInBytes)
            {
                Cache.Clear();
                LRUList.Clear();
                TotalCacheSizeInBytes = 0;
                ItemsEvictedSinceLastGC = 0;
                return;
            }

            // Evict until we have enough space.
            while (TotalCacheSizeInBytes + soundSize > MaxCacheSizeInBytes && LRUList.Count > 0)
            {
                EvictLeastRecentlyUsed(null); // No protected key when adding new items.
            }
        }
    }

    /// Description
    /// <summary>
    ///     Adds a sound to the cache, evicting LRU items until enough space is available.
    /// </summary>
    private void AddToCache(String fileName, CachedSound sound)
    {
        Int64 soundSize = sound.AudioData.Length * sizeof(Single) + ObjectOverhead;
        EnsureCacheSpace(soundSize);

        lock (CacheLock)
        {
            CacheEntry entry = new(sound);
            Cache[fileName] = entry;
            TotalCacheSizeInBytes += entry.SizeInBytes;
            LRUList.AddLast(fileName); // Mark as most recent.
        }
    }

    /// Description
    /// <summary>
    ///     Marks a sound as accessed (moves it to the end of the LRU list) and evicts
    ///     items if the cache exceeds its limit (protecting the accessed item).
    /// </summary>
    private void MarkAccessed(String fileName)
    {
        lock (CacheLock)
        {
            // Move to end (mark as most recent).
            if (LRUList.Remove(fileName))
            {
                LRUList.AddLast(fileName);
            }

            // Evict items if cache is over the limit, but protect the one we just accessed.
            while (TotalCacheSizeInBytes > MaxCacheSizeInBytes && LRUList.Count > 1)
            {
                EvictLeastRecentlyUsed(fileName);
            }
        }
    }

    /// Description
    /// <summary>
    ///     Handles the <see cref="MixingSampleProvider.MixerInputEnded"/> event.
    /// </summary>
    private void OnMixerInputEnded(Object? sender, SampleProviderEventArgs e)
    {
        // OnMixerInputEnded gets invoked before the corresponding source is removed
        // from the mixer, so there should be exactly one source left if this was the last.
        if (Mixer.MixerInputs.Count() == 1)
            AllInputEnded?.Invoke(this, EventArgs.Empty);
    }

    /// Description
    /// <summary>
    ///     Converts the input sample provider to the mixer's channel count.
    /// </summary>
    /// <param name="input">The input provider.</param>
    /// <returns>A provider with the correct channel count.</returns>
    /// <exception cref="NotImplementedException">
    ///     Thrown if the channel conversion is not implemented.
    /// </exception>
    private ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
    {
        if (input.WaveFormat.Channels == Mixer.WaveFormat.Channels)
            return input;

        if (input.WaveFormat.Channels == 1 && Mixer.WaveFormat.Channels == 2)
            return new MonoToStereoSampleProvider(input);

        throw new NotImplementedException("Not yet implemented this channel count conversion");
    }

    /// Description
    /// <summary>
    ///     Adds an input to the mixer, applying resampling and channel conversion.
    /// </summary>
    private void AddMixerInput(ISampleProvider input)
    {
        WdlResamplingSampleProvider resampled = new(input, Mixer.WaveFormat.SampleRate);
        Mixer.AddMixerInput(ConvertToRightChannelCount(resampled));
    }

    #endregion

    #region Public Methods

    /// Description
    /// <summary>
    ///     Initialises the audio output device.
    /// </summary>
    /// <param name="deviceNumber">The zero‑based index of the audio device to use.</param>
    public void Init(Int32 deviceNumber)
    {
        OutputDevice?.Dispose();

        WaveOutEvent output = new()
        {
            DeviceNumber = deviceNumber
        };
        output.Init(Mixer);
        output.Play();

        OutputDevice = output;
    }

    /// Description
    /// <summary>
    ///     Plays a sound from a file, caching it for future use.
    /// </summary>
    /// <param name="fileName">The full path to the audio file.</param>
    public void PlaySound(String fileName)
    {
        LastPlayTime = DateTime.Now;

        // Try to get from cache.
        if (Cache.TryGetValue(fileName, out CacheEntry? entry))
        {
            MarkAccessed(fileName);
            PlaySound(entry.Sound);
            return;
        }

        // Not cached – load, cache, then play.
        CachedSound sound = new(fileName);
        AddToCache(fileName, sound);
        PlaySound(sound);
    }

    /// Description
    /// <summary>
    ///     Plays a pre‑cached sound.
    /// </summary>
    /// <param name="sound">The <see cref="CachedSound"/> to play.</param>
    public void PlaySound(CachedSound sound)
    {
        AddMixerInput(new CachedSoundSampleProvider(sound));
    }

    /// Description
    /// <summary>
    ///     Stops all currently playing sounds.
    /// </summary>
    public void StopAllSounds()
    {
        Mixer.RemoveAllMixerInputs();
    }

    /// Description
    /// <summary>
    ///     Disposes the audio engine and releases all resources.
    /// </summary>
    public void Dispose()
    {
        IdleTimer?.Dispose();
        OutputDevice?.Dispose();
        OutputDevice = null;

        lock (CacheLock)
        {
            Cache.Clear();
            LRUList.Clear();
            TotalCacheSizeInBytes = 0;
            ItemsEvictedSinceLastGC = 0;
        }

        ForceGarbageCollection();
        GC.SuppressFinalize(this);
    }

    #endregion
}