using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text.Json;

namespace moos.Models
{
    public class Playlist(): IEquatable<Playlist>
    {
        public int Id = 0;
        public string Name { get; set; } = "Untitled";
        public string Description { get; set; }
        public DateTime Modified { get; set; }
        public string? FilePath { get; set; }
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
            else
            {
                item.IsPlaying = true;
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
            HashSet<int> idsToRemove = [.. removeIds];
            int currentTrackId = GetCurrentPlayingId();
            List<PlaylistItem> remaining = [.. CurrentPlaylist.Where(item => !idsToRemove.Contains(item.Id))];
            // I had to think about this
            // Find new playing index
            PlayerPosition = remaining.FindIndex(item => item.Id == currentTrackId);

            if(PlayerPosition == -1)
            {
                // Move to previous unremoved index
                var prev = CurrentPlaylist
                            .Take(PlayerPosition)                                       // Takes sequence before playerposition index
                            .Reverse()                                                  // Flip sequence, going from closest to farthest
                            .FirstOrDefault(item => !idsToRemove.Contains(item.Id));    // Finds closest unremoved element
                if (prev is not null) 
                    PlayerPosition = remaining.FindIndex(item => item.Id == prev.Id);     
                else if (remaining.Count > 0) PlayerPosition = 0;   // If no previous track found, the first remaining track
            }

            CurrentPlaylist = new ObservableCollection<PlaylistItem>(remaining);

            return CurrentPlaylist;
        }

        public PlaylistItem? ReturnTrack(int? newPlayerId = null)
        {
            CurrentPlaylist[PlayerPosition].IsPlaying = false;
            if(newPlayerId is null && PlayerPosition < CurrentPlaylist!.Count - 1)
            {
                PlayerPosition++;
            }
            else if(newPlayerId == -1 && PlayerPosition > 0)
            {
                PlayerPosition--;
            }
            else if(newPlayerId is not null && newPlayerId.Value != -1)
            {
                PlayerPosition = GetPositionById(newPlayerId.Value);
            }
            else
            {
                return null;
            }
            CurrentPlaylist[PlayerPosition].IsPlaying = true;

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
            else if (currentIndex > PlayerPosition && newIndex <= PlayerPosition)
            {
                PlayerPosition++;
            }
            else if(currentIndex < PlayerPosition && newIndex > PlayerPosition)
            {
                PlayerPosition--;
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

        public int GetPositionById(int id)
        {
            int newPosition = CurrentPlaylist!.IndexOf(CurrentPlaylist!.Single(item => item.Id == id));
            return newPosition;
        }

        public bool IsSavedPlaylist()
        {
            return String.IsNullOrEmpty(FilePath);
        }

        public bool Equals(Playlist? other)
        {
            if (other is null) return false;

            if (string.IsNullOrEmpty(this.Name)) return false;

            if (!string.Equals(this.Name.Trim(), other.Name.Trim(), StringComparison.Ordinal)) return false;

            if (this.CurrentPlaylist.Count != other.CurrentPlaylist.Count) return false;

            if (JsonSerializer.Serialize(this.CurrentPlaylist) != JsonSerializer.Serialize(other.CurrentPlaylist)) return false;

            return true;
        }
    }
}
