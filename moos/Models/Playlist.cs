using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using moos.Services;
using NAudio.Wave;

namespace moos.Models
{
    public class Playlist
    {
        public ObservableCollection<Track>? CurrentPlaylist;
        private int PlayerPosition = 0;

        public void AddTrack(Track track)
        {
            if(CurrentPlaylist is null)
            {
                CurrentPlaylist = new ObservableCollection<Track>();
            }

            CurrentPlaylist.Add(track);
        }

        public void RemoveTrack(string filePath)
        {
            if(CurrentPlaylist is not null)
            {
                CurrentPlaylist.Remove(CurrentPlaylist.First(track => track.FilePath == filePath));
                if (CurrentPlaylist.Count == 0)
                {
                    CurrentPlaylist = null;
                }
            }
        }

        public Track? ReturnTrack(int? newPlayerPosition = null)
        {
            if(newPlayerPosition is null && PlayerPosition < CurrentPlaylist!.Count - 1)
            {
                PlayerPosition++;
            }
            else if(newPlayerPosition == -1 && PlayerPosition > 0)
            {
                PlayerPosition--;
            }
            else if(newPlayerPosition is not null && newPlayerPosition.Value != -1)
            {
                PlayerPosition = newPlayerPosition.Value;
            }
            else
            {
                return null;
            }
            
            return CurrentPlaylist!.ElementAt(PlayerPosition);        
        }


    }
}
