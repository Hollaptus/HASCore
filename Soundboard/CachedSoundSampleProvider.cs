using NAudio.Wave;

namespace HASCore.Soundboard;

/// Description
/// <summary>
///		Provides sample data from a <see cref="CachedSound"/> for playback through
///		the mixer.
/// </summary>
/// Description
/// <remarks>
///		Initializes a new instance of the <see cref="CachedSoundSampleProvider"/> class.
/// </remarks>
/// <param name="cachedSound">The cached sound to provide samples from.</param>
internal class CachedSoundSampleProvider(CachedSound cachedSound) : ISampleProvider
{
	#region Private Fields

	private readonly CachedSound _cachedSound = cachedSound;
	private Int64 _position;

    #endregion

    #region Public Properties

    /// Description
    /// <summary>
    ///		Gets the cached sound this provider reads from.
    /// </summary>
    public CachedSound Sound => _cachedSound;

	/// Description
	/// <summary>
	///		Gets the wave format of the cached sound.
	/// </summary>
	public WaveFormat WaveFormat => _cachedSound.WaveFormat;

	#endregion

	#region Public Methods

	/// Description
	/// <summary>
	///		Reads a sequence of samples from the cached sound into the supplied buffer.
	/// </summary>
	/// <param name="buffer">The buffer to write samples into.</param>
	/// <param name="offset">The offset within <paramref name="buffer"/> to start writing.</param>
	/// <param name="count">The maximum number of samples to read.</param>
	/// <returns>The number of samples actually read.</returns>
	public Int32 Read(Single[] buffer, Int32 offset, Int32 count)
	{
		if (_position >= _cachedSound.AudioData.Length)
			return 0;

		Int64 availableSamples = _cachedSound.AudioData.Length - _position;
		Int32 samplesToCopy = (Int32)Math.Min(availableSamples, count);
		if (samplesToCopy <= 0)
			return 0;

		Array.Copy(_cachedSound.AudioData, _position, buffer, offset, samplesToCopy);

		_position += samplesToCopy;

		return samplesToCopy;
	}

	#endregion
}