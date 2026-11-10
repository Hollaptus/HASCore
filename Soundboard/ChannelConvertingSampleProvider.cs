using NAudio.Wave;

namespace HASCore.Soundboard;

/// Description
/// <summary>
///		Generic channel converter that downmixes any input to mono and
///		replicates the result across the requested number of output channels.
/// </summary>
/// Description
/// <remarks>
///		Initialises a new instance of the <see cref="ChannelConvertingSampleProvider"/> class.
/// </remarks>
/// <param name="source">The source sample provider.</param>
/// <param name="outputChannels">The number of output channels.</param>
internal class ChannelConvertingSampleProvider(ISampleProvider source, Int32 outputChannels) : ISampleProvider
{
	/// Description
	/// <summary>
	///		The source sample provider.
	/// </summary>
	private readonly ISampleProvider _source = source;

	/// Description
	/// <summary>
	///		The number of channels in the source.
	/// </summary>
	private readonly Int32 _inputChannels = source.WaveFormat.Channels;

	/// Description
	/// <summary>
	///		The number of channels to produce.
	/// </summary>
	private readonly Int32 _outputChannels = outputChannels;

	/// Description
	/// <summary>
	///		Buffer for reading source samples.
	/// </summary>
	private Single[]? _sourceBuffer;

    /// Description
    /// <summary>
    ///		Gets the wave format produced by this provider.
    /// </summary>
    public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(source.WaveFormat.SampleRate, outputChannels);

    /// Description
    /// <summary>
    ///		Reads converted samples into the supplied buffer.
    /// </summary>
    /// <param name="buffer">The destination buffer.</param>
    /// <param name="offset">The offset into the destination buffer.</param>
    /// <param name="count">The number of samples requested.</param>
    /// <returns>The number of samples written.</returns>
    public Int32 Read(Single[] buffer, Int32 offset, Int32 count)
	{
		Int32 outputFrames = count / _outputChannels;
		Int32 inputCount = outputFrames * _inputChannels;
		if (_sourceBuffer is null || _sourceBuffer.Length < inputCount)
			_sourceBuffer = new Single[inputCount];

		Int32 read = _source.Read(_sourceBuffer, 0, inputCount);
		Int32 framesRead = read / _inputChannels;

		for (Int32 frame = 0; frame < framesRead; frame++)
		{
			Single mono = 0f;
			for (Int32 channel = 0; channel < _inputChannels; channel++)
				mono += _sourceBuffer[frame * _inputChannels + channel];
			mono /= _inputChannels;

			for (Int32 channel = 0; channel < _outputChannels; channel++)
				buffer[offset + frame * _outputChannels + channel] = mono;
		}

		return framesRead * _outputChannels;
	}
}
