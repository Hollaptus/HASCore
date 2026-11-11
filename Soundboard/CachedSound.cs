using NAudio.Wave;
using NAudio.Wave.SampleProviders;

// https://mark-dot-net.blogspot.de/2014/02/fire-and-forget-audio-playback-with.html
// Mark Heath 2014

namespace HASCore.Soundboard;

/// Description
/// <summary>
///		Represents a sound fully decoded and resampled into memory as
///		floating-point samples, ready for low-latency playback through
///		the audio engine.
/// </summary>
///
/// Additional information
/// <remarks>
///		The constructor reads the entire audio file, resamples it to the
///		target format (44.1 kHz stereo by default), and stores the result
///		in <see cref="AudioData"/>. <see cref="AudioPlaybackEngine"/> uses
///		this class to play sounds without re-reading from disk on each
///		trigger. Resampling at load time ensures every cached sound is in a
///		uniform format for the mixer.
/// </remarks>
internal class CachedSound
{
	/// Description
	/// <summary>
	///		Gets the decoded and resampled audio sample data as an array of
	///		single-precision floating-point values, in the target format.
	/// </summary>
	public Single[] AudioData { get; private set; }

	/// Description
	/// <summary>
	///		Gets the <see cref="WaveFormat"/> of the decoded audio data,
	///		matching the target resampling format.
	/// </summary>
	public WaveFormat WaveFormat { get; private set; }

	/// Description
	/// <summary>
	///		The target <see cref="WaveFormat"/> all sounds are resampled to
	///		during loading. Matches the default
	///		<see cref="AudioPlaybackEngine"/> mixer format (44.1 kHz stereo).
	/// </summary>
	private static readonly WaveFormat TargetFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

	/// Description
	/// <summary>
	///		Initializes a new instance of the <see cref="CachedSound"/> class by
	///		decoding, resampling, and channel-converting the specified audio
	///		file into memory.
	/// </summary>
	///
	/// Parameters
	/// <param name="audioFileName">The path to the audio file to load and decode.</param>
	public CachedSound(String audioFileName)
	{
		// Open the source audio file for reading. AudioFileReader decodes
		// the file into IEEE float samples on demand.
		using AudioFileReader audioFileReader = new (audioFileName);

		// Resample the source to the target sample rate so every cached
		// sound matches the mixer's output rate.
		WdlResamplingSampleProvider resampler = new (audioFileReader, TargetFormat.SampleRate);

		// Convert channel count to the target (e.g. mono to stereo, or
		// stereo to mono) so every cached sound uses a uniform layout.
		ChannelConvertingSampleProvider channelConverter = new (resampler, TargetFormat.Channels);

		// Record the final format that the audio data is stored in.
		WaveFormat = TargetFormat;

		// Pre-size the list based on the source file length divided by 4
		// bytes per float to avoid reallocations while reading.
		List<Single> wholeFile = [with((Int32)(audioFileReader.Length / 4))];

		// Reusable read buffer sized to one second of target-format audio.
		Single[] readBuffer = new Single[TargetFormat.SampleRate * TargetFormat.Channels];
		Int32 samplesRead;

		// Read the decoded, resampled, channel-converted audio in chunks
		// until the entire file has been consumed.
		while ((samplesRead = channelConverter.Read(readBuffer, 0, readBuffer.Length)) > 0)
			wholeFile.AddRange(readBuffer.Take(samplesRead));

		// Flatten the accumulated samples into a single array for playback.
		AudioData = [.. wholeFile];
	}
}