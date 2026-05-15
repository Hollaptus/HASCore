using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace JNSoundboardCore
{
    class AudioPlaybackEngine : IDisposable
    {
        public static readonly AudioPlaybackEngine Instance = new(44100, 2);

        // private IWavePlayer OutputDevice;
        private WaveOutEvent? OutputDevice;
        private readonly MixingSampleProvider Mixer;
        private readonly Dictionary<String, CachedSound> CachedSounds = [];
        public event EventHandler? AllInputEnded;

        public AudioPlaybackEngine(Int32 sampleRate = 44100, Int32 channelCount = 2)
        {
            Mixer = new(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount))
            {
                ReadFully = true
            };
            Mixer.MixerInputEnded += OnMixerInputEnded;
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

            WaveOutEvent output = new()
            {
                DeviceNumber = deviceNumber
            };
            output.Init(Mixer);
            output.Play();

            OutputDevice = output;
        }

        public void PlaySound(String fileName)
        {
            // AudioFileReader input = new AudioFileReader(fileName);
            if (!CachedSounds.TryGetValue(fileName, out CachedSound? cachedSound))
            {
                cachedSound = new CachedSound(fileName);
                CachedSounds.Add(fileName, cachedSound);
            }

            PlaySound(cachedSound);
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
            WdlResamplingSampleProvider resampled = new(input, Mixer.WaveFormat.SampleRate);
            Mixer.AddMixerInput(ConvertToRightChannelCount(resampled));
        }

        public void Dispose()
        {
            OutputDevice?.Dispose();
            OutputDevice = null;
        }
    }
}
