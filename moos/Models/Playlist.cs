using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using moos.Services;
using NAudio.Wave;

namespace moos.Models
{
    public class Playlist
    {
        private ObservableCollection<Track>? currentPlaylist;
        private Player playerInstance = new Player();
        private int playerPosition = 0;

        public void AddTrack(Track track)
        {
            if(currentPlaylist == null)
            {
                currentPlaylist = new ObservableCollection<Track>();
            }

            currentPlaylist.Add(track);
        }

        public void RemoveTrack(string filePath)
        {
            if(currentPlaylist != null)
            {
                currentPlaylist.Remove(currentPlaylist.First(track => track.FilePath == filePath));
                if (currentPlaylist.Count == 1)
                {
                    currentPlaylist = null;
                }
            }
        }

        public void PlayThrough(int? newPlayerPosition = null)
        {
            if(newPlayerPosition != null)
            {
                playerPosition = (int)newPlayerPosition;
            }

            if (currentPlaylist != null && playerPosition < currentPlaylist.Count())
            {
                List<string> trackPaths = currentPlaylist.Select(track => track.FilePath).ToList();
                playerInstance.PlayTrack(trackPaths[playerPosition]);

                playerInstance.TrackFinished += OnTrackFinished;
            }
        }

        private void OnTrackFinished(object? sender, StoppedEventArgs args)
        {
            playerPosition++;
            PlayThrough();
        }

        public void ResumeTrack()
        {
            playerInstance.ResumeTrack();
        }

        public void PauseTrack()
        {
            playerInstance.PauseTrack();
        }

        public void StopTrack()
        {
            playerInstance.StopTrack();
        }
    }
}
