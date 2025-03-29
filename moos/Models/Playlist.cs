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
        public string Name { get; private set; } = "Untitled";
        private ObservableCollection<Track> CurrentPlaylist = [];
        private int PlayerPosition = 0;

        public ObservableCollection<Track> AddTrack(Track track)
        {
            CurrentPlaylist.Add(track);
            return CurrentPlaylist;
        }

        public ObservableCollection<Track> RemoveTrack(string filePath)
        {
            CurrentPlaylist.Remove(CurrentPlaylist.First(track => track.FilePath == filePath));
            return CurrentPlaylist;
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

        public ObservableCollection<Track> ReorderPlaylist(string filePath, int newIndex)
        {
            int currentIndex = CurrentPlaylist.IndexOf(CurrentPlaylist.First(track => track.FilePath == filePath));
            CurrentPlaylist.Move(currentIndex, newIndex);
            return CurrentPlaylist;
        }
    }
}
