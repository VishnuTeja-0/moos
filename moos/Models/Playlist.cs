using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using moos.Services;
using NAudio.Wave;

namespace moos.Models
{
    public class Playlist
    {
        public int Id = 0;
        public string Name { get; set; } = "Untitled";
        public string Description { get; set; }
        public DateTime Modified { get; set; }
        public ObservableCollection<PlaylistItem> CurrentPlaylist = [];
        private int PlayerPosition = 0;

        public ObservableCollection<PlaylistItem> AddTrack(Track track, float speed, float pitch)
        {
            var item = new PlaylistItem(track, speed, pitch);
            item.IsActive = File.Exists(track.FilePath);
            if(CurrentPlaylist.Count > 0)
            {
                item.Id = CurrentPlaylist[CurrentPlaylist.Count - 1].Id + 1;
            }
            CurrentPlaylist.Add(item);
            return CurrentPlaylist;
        }

        public ObservableCollection<PlaylistItem> AddTracks(IEnumerable<Track> tracks)
        {
            foreach (var track in tracks)
            {
                AddTrack(track, Constants.DefaultPlayingSpeed, Constants.DefaultPlayingPitch);
            }
            return CurrentPlaylist;
        }

        public ObservableCollection<PlaylistItem> RemoveTracks(List<int> removeIds)
        {
            foreach(var id in removeIds)
            {
                CurrentPlaylist.Remove(CurrentPlaylist.Single(item => item.Id == id));
            }
            return CurrentPlaylist;
        }

        public PlaylistItem? ReturnTrack(int? newPlayerPosition = null)
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

        public ObservableCollection<PlaylistItem> ReorderPlaylist(int currentIndex, int newIndex)
        {
            (CurrentPlaylist[currentIndex].Id, CurrentPlaylist[newIndex].Id)
                = (CurrentPlaylist[newIndex].Id, CurrentPlaylist[currentIndex].Id);
            CurrentPlaylist.Move(currentIndex, newIndex);
            if (currentIndex == PlayerPosition)
            {
                PlayerPosition = newIndex;
            }
            return CurrentPlaylist;
        }

        public ObservableCollection<PlaylistItem> UpdatePlaylistTrack(string filePath, float updatedSpeed, float updatedPitch)
        {
            var item = CurrentPlaylist!.ElementAt(PlayerPosition);
            if(item.Track.FilePath == filePath)
            {
                item.Speed = updatedSpeed;
                item.Pitch = updatedPitch;
            }
            return CurrentPlaylist;
        }

        public int GetCurrentPlayingId()
        {
            PlaylistItem item = CurrentPlaylist!.ElementAt(PlayerPosition);
            return item.Id;
        }
    }
}
