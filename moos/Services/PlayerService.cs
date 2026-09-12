using System.Runtime.InteropServices;
using moos.Interfaces.Services;
using NAudio.Wave;
using NAudio.Wave.Alsa;
using NAudio.Wave.SampleProviders;
using VarispeedDemo.SoundTouch;
using NAudio.SoundFile;


namespace moos.Services
{
    public class PlayerService : IAudioPlayer
    {
        private IWavePlayer? outputDevice;
        private SoundFileReader? audioFile;
        private SmbPitchShiftingSampleProvider? pitch;
        private VarispeedSampleProvider? speed;


        public void PlayTrack(string filePath)
        {
            try
            {
                if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    outputDevice = new WasapiPlayerBuilder()
                        .WithLowLatency()
                        .Build();
                }
                else
                {
                    throw new PlatformNotSupportedException();
                }
                
                audioFile = new SoundFileReader(filePath);

                speed = new VarispeedSampleProvider(
                        audioFile.ToSampleProvider(),
                        100,
                        new SoundTouchProfile(true, false)
                        );

                pitch = new SmbPitchShiftingSampleProvider(
                    speed,
                    2048,
                    10,
                    1.0f);

                outputDevice.Init(pitch);
                
                outputDevice.Play();
            }
            catch (Exception ex)
            {
                // Logging and error bubble
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public void ResumeTrack()
        {
            outputDevice!.Play();
        }

        public void PauseTrack()
        {
            outputDevice!.Pause();
        }

        public void StopTrack()
        {
            outputDevice!.Stop();
            pitch = null;
            speed = null;
            outputDevice!.Dispose();
            outputDevice = null;
            audioFile!.Dispose();
            audioFile = null;
        }

        public void SetVolume(float newVolume)
        {
            if(outputDevice is not null)
            {
                float newVolumeValue = newVolume / 100f;
                outputDevice.Volume = newVolumeValue;
            }
        }

        public void SeekToPosition(double seconds)
        {
            if (outputDevice is not null && audioFile is not null)
            {
                long newPosition = (long)(seconds * audioFile.WaveFormat.AverageBytesPerSecond);

                if((newPosition % audioFile.WaveFormat.BlockAlign) != 0)
                {
                    newPosition -= newPosition % audioFile.WaveFormat.BlockAlign;
                }

                newPosition = Math.Clamp(newPosition, 0, audioFile.Length);
                
                audioFile.Position = newPosition;
                speed!.Reposition();
            }
        }

        public double GetPosition()
        {
            double time = 0;

            if (outputDevice is not null && audioFile is not null)
            {
                time = audioFile.CurrentTime.TotalSeconds;
            }

            return time;
        }

        public void SetSpeed(float newSpeed)
        {
            if (outputDevice is not null && audioFile is not null)
            {
                float speedChange = newSpeed / 100;
                speed!.PlaybackRate = speedChange;
            }
        }

        public void SetPitch(float newPitch)
        {
            if (outputDevice is not null && audioFile is not null)
            {
                float semitoneChange = (float)Math.Pow(2, (newPitch / 0.5f) / 12);
                pitch!.PitchFactor = semitoneChange;
            }
        }
    }
}