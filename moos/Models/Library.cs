using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace moos.Models
{
    public class Library : ReactiveObject
    {
        private ObservableCollection<Track> LocalLibraryCollection { get; set; }

        public Library() 
        {
            LocalLibraryCollection = [];
            
        }

        public ObservableCollection<Track> LoadLocalCollection(string folderPath)
        {
            LocalLibraryCollection = [];

            if (Directory.Exists(folderPath))
            {
                foreach (string filePath in Directory.GetFiles(folderPath, "*.mp3"))
                {
                    var tfile = TagLib.File.Create(filePath);
                    string title = tfile.Tag.Title;
                    TimeSpan duration = tfile.Properties.Duration;
                    ObservableCollection<string> artists = new ObservableCollection<string>(tfile.Tag.Performers);
                    string album = tfile.Tag.Album;
                    uint year = tfile.Tag.Year;
                    LocalLibraryCollection.Add(new Track(title, filePath, duration, artists, album, year));
                }
            }
            else 
            {
                throw new DirectoryNotFoundException($"Local folder not found at \"{folderPath}\"");
            }

            return LocalLibraryCollection;
        }

        public async void EditTrackMetadata(Track updatedTrack, string folderPath)
        {
            string filePath = updatedTrack.FilePath;

            var tfile = TagLib.File.Create(filePath);
            tfile.Tag.Title = updatedTrack.Title;
            tfile.Tag.Performers = updatedTrack.Artists!.ToArray();
            tfile.Tag.Album = updatedTrack.Album;
            tfile.Tag.Year = (uint)updatedTrack.Year;
            tfile.Save();
            await Task.Delay(500);

            //_ = Task.Run(async () =>
            //{
            //    while (true)
            //    {
            //        await Task.Delay(TimeSpan.FromSeconds(1));

            //        Dispatcher.UIThread.Post(() =>
            //        {
            //            LoadLocalCollection(folderPath);
            //            this.RaisePropertyChanged(nameof(LocalLibraryCollection));
            //        });
            //    }
            //});

            //LoadLocalCollection(folderPath);
        }

        public void DownloadSong(string url)
        {

        }
    }
}
