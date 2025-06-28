using Avalonia.Platform.Storage;
using moos.Models;
using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace moos.Services
{
    public class SavedPlaylist
    {
        public string Name { get; set; } = "Untitled";
        public List<SavedPlaylistTrack> Tracks { get; set; } = new();
        public string DisplayModifiedDate { get; set; } = DateTime.Now.ToString("MMM dd, yyyy");
        public string? FilePath { get; set; }
    }

    public class SavedPlaylistTrack
    {
        public string Title { get; set; } = "";
        public string FilePath { get; set; } = "";
        public float Speed { get; set; }
        public float Pitch { get; set; }
    }

    public class PlaylistService
    {
        public async void SavePlaylist(Playlist playlist)
        {
            var save = new SavedPlaylist
            {
                Name = playlist.Name,
                Tracks = playlist.CurrentPlaylist.Select(item => new SavedPlaylistTrack
                {
                    Title = item.Track.Title,
                    FilePath = item.Track.FilePath,
                    Speed = item.Speed,
                    Pitch = item.Pitch
                }).ToList(),
                DisplayModifiedDate = DateTime.Now.ToString("MMM dd, yyyy")
            };

            var json = JsonSerializer.Serialize(save, new JsonSerializerOptions { WriteIndented = true });

            Directory.CreateDirectory(Constants.PlaylistFolder);
            string fileName = GetValidFilename(save.Name);
                
            var filepath = Path.Combine(Constants.PlaylistFolder, $"{fileName}.json");
            await File.WriteAllTextAsync(filepath, json);
        }

        public ObservableCollection<SavedPlaylist> GetPlaylistsWithPaths()
        {
            var savedPlaylists = new ObservableCollection<SavedPlaylist>() { };
            if (Directory.Exists(Constants.PlaylistFolder))
            {
                foreach (string filePath in Directory.GetFiles(Constants.PlaylistFolder, "*.json"))
                {
                    savedPlaylists.Add(
                        new SavedPlaylist 
                        { 
                            Name = Path.GetFileNameWithoutExtension(filePath),
                            DisplayModifiedDate = File.GetLastWriteTime(filePath).ToString("MMM dd, yyyy"),
                            FilePath = filePath
                        }
                    );
                }
            }
            return savedPlaylists;
        }

        public async Task<Playlist> LoadPlaylist(string playListPath, Library library)
        {
            if (!File.Exists(playListPath))
                throw new FileNotFoundException("Playlist File not found");

            var json = await File.ReadAllTextAsync(playListPath);
            var saved = JsonSerializer.Deserialize<SavedPlaylist>(json) ?? throw new InvalidOperationException();

            var playlist = new Playlist();
            playlist.Name = saved.Name;
            playlist.Modified = DateTime.ParseExact(saved.DisplayModifiedDate, "MMM dd, yyyy", CultureInfo.InvariantCulture);
            var trackCollection = library.LoadLocalCollection(Constants.LibraryFolder);
            foreach(var item in saved.Tracks)
            {
                var track = trackCollection.FirstOrDefault(track => track.FilePath == item.FilePath, null);
                if(track is null) 
                {
                    track = new Track(item.Title, item.FilePath, TimeSpan.Zero, []);
                }
                playlist.AddTrack(track, item.Speed, item.Pitch);
            }

            return playlist;
        }

        private string GetValidFilename(string playlistName)
        {
            string result = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                result = String.Join("_", playlistName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
            }

            return result;
        }
    }
}
