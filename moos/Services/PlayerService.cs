using NAudio.Wave;
using System;
using moos.Interfaces;


namespace moos.Services
{
    public class PlayerService : IAudioPlayer
    {
        private WaveOutEvent? outputDevice;
        private AudioFileReader? audioFile;

        public void PlayTrack(string filePath)
        {
            try
            {
                outputDevice = new WaveOutEvent();
                audioFile = new AudioFileReader(filePath);
                outputDevice.Init(audioFile);
                
                outputDevice.Play();
            }
            catch (Exception ex)
            {
                // Logging and error display
                Console.WriteLine(ex.Message);
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
            outputDevice.Dispose();
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
    }
}