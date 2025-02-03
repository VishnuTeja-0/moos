using Avalonia.Controls.Primitives;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace moos.Services
{
    public class Player
    {
        private WaveOutEvent? outputDevice;

        public void PlayTrack(string filePath)
        {
            if (outputDevice != null)
            {
                StopTrack();
            }

            try
            {
                outputDevice = new WaveOutEvent();
                var audioFile = new AudioFileReader(filePath);
                outputDevice.Init(audioFile);
                outputDevice.Play();

                outputDevice.PlaybackStopped += TrackFinished;
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
        }

        public event EventHandler<StoppedEventArgs>? TrackFinished;    }
}
