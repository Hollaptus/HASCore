using System.Collections.Concurrent;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Timer = System.Threading.Timer;

namespace HASCore.Soundboard;

class AudioPlaybackEngine : IDisposable
{
    public static readonly AudioPlaybackEngine Instance = new (44100, 2);

    // Configuration
    private const Int64 ObjectOverhead = 200; // Bytes 
    private const Int64 MaxCacheSizeInBytes = 100L * 1024 * 1024; // 100 MB
    private static readonly TimeSpan IdleTimeout = TimeSpan.FromMinutes(60);

    private WaveOutEvent? OutputDevice;
    private readonly MixingSampleProvider Mixer;
    
    // Cache storage with memory tracking
    private readonly ConcurrentDictionary<String, CacheEntry> Cache = new ();
    // Maintains access order (head = oldest, tail = newest)
    private readonly LinkedList<String> LRUList = new ();
    private Int64 TotalCacheSizeInBytes = 0;
    private readonly Lock CacheLock = new ();

    private DateTime LastPlayTime = DateTime.Now;
    private readonly Timer? IdleTimer;

    public event EventHandler? AllInputEnded;

    private class CacheEntry(CachedSound sound)
    {
        public CachedSound Sound { get; } = sound;
        // Calculate size: 
        // float[] length * 4 bytes/float + overhead for creating the object in memory
        public Int64 SizeInBytes { get; } = sound.AudioData.Length * sizeof(Single) + ObjectOverhead;
    }

    public AudioPlaybackEngine(Int32 sampleRate = 44100, Int32 channelCount = 2)
    {
        Mixer = new (WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount))
        {
            ReadFully = true
        };
        Mixer.MixerInputEnded += OnMixerInputEnded;

        // Start a timer that checks every minute for idle cleanup
        IdleTimer = new Timer(
            _ => CheckIdleAndClearCache(),
            null,
            TimeSpan.FromSeconds(60),
            TimeSpan.FromSeconds(60)
        );
    }

    private void CheckIdleAndClearCache()
    {
        if (DateTime.Now - LastPlayTime > IdleTimeout)
        {
            lock (CacheLock)
            {
                Cache.Clear();
                LRUList.Clear();
                TotalCacheSizeInBytes = 0;
            }
        }
    }

    // Adds a sound to the cache, evicting LRU items until enough space is available.
    private void AddToCache(String fileName, CachedSound sound)
    {
        Int64 soundSize = sound.AudioData.Length * sizeof(Single);

        lock (CacheLock)
        {
            // If sound is larger than the entire cache, clear everything and store it anyway.
            if (soundSize > MaxCacheSizeInBytes)
            {
                Cache.Clear();
                LRUList.Clear();
                TotalCacheSizeInBytes = 0;
                // Add it directly (we'll handle the rest below)
            }

            // Evict until we have enough space
            while (TotalCacheSizeInBytes + soundSize > MaxCacheSizeInBytes && LRUList.Count > 0)
            {
                // Get the least recently used key (first in list)
                String oldestKey = LRUList.First!.Value;
                if (Cache.TryRemove(oldestKey, out CacheEntry? removed))
                {
                    TotalCacheSizeInBytes -= removed.SizeInBytes;
                    LRUList.RemoveFirst();
                }
                else
                {
                    // In case of inconsistency, remove from list and continue
                    LRUList.RemoveFirst();
                }
            }

            // If after eviction we still can't fit we just allow it and ignore the memory limit for this item.
            // The total will exceed the limit, but future additions will evict this item.
            CacheEntry entry = new (sound);
            Cache[fileName] = entry;
            TotalCacheSizeInBytes += entry.SizeInBytes;
            // Mark as most recent.
            LRUList.AddLast(fileName); 
        }
    }

    // Mark a sound as accessed (move to end of LRU list)
    private void MarkAccessed(string fileName)
    {
        lock (CacheLock)
        {
            if (LRUList.Remove(fileName))
            {
                LRUList.AddLast(fileName);
            }
        }
    }

    private void OnMixerInputEnded(Object? sender, SampleProviderEventArgs e)
    {
        // check if there are any inputs left
        // OnMixerInputEnded gets invoked before the corresponding source is removed from the List so there should be exactly one source left
        if (Mixer.MixerInputs.Count() == 1)
            AllInputEnded?.Invoke(this, EventArgs.Empty);
    }

    public void Init(Int32 deviceNumber)
    {
        OutputDevice?.Dispose();

        WaveOutEvent output = new ()
        {
            DeviceNumber = deviceNumber
        };
        output.Init(Mixer);
        output.Play();

        OutputDevice = output;
    }

    public void PlaySound(String fileName)
    {
        LastPlayTime = DateTime.Now;

        // Try to get from cache
        if (Cache.TryGetValue(fileName, out CacheEntry? entry))
        {
            MarkAccessed(fileName);
            PlaySound(entry.Sound);
            return;
        }

        // Not cached – load, cache, then play
        CachedSound sound = new (fileName);
        AddToCache(fileName, sound);
        PlaySound(sound);
    }

    public void PlaySound(CachedSound sound)
    {
        AddMixerInput(new CachedSoundSampleProvider(sound));
    }

    public void StopAllSounds()
    {
        Mixer.RemoveAllMixerInputs();
    }

    private ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
    {
        if (input.WaveFormat.Channels == Mixer.WaveFormat.Channels)
            return input;
        
        if (input.WaveFormat.Channels == 1 && Mixer.WaveFormat.Channels == 2)
            return new MonoToStereoSampleProvider(input);
        
        throw new NotImplementedException("Not yet implemented this channel count conversion");
    }

    private void AddMixerInput(ISampleProvider input)
    {
        WdlResamplingSampleProvider resampled = new (input, Mixer.WaveFormat.SampleRate);
        Mixer.AddMixerInput(ConvertToRightChannelCount(resampled));
    }

    public void Dispose()
    {
        IdleTimer?.Dispose();
        OutputDevice?.Dispose();
        OutputDevice = null;
        Cache.Clear();
        LRUList.Clear();
    }
}