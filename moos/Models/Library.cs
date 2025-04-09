using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


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
                    string title = tfile.Tag.Title ?? Path.GetFileNameWithoutExtension(filePath);
                    TimeSpan duration = tfile.Properties.Duration;
                    ObservableCollection<string> artists = new ObservableCollection<string>(tfile.Tag.Performers);
                    string album = tfile.Tag.Album;
                    string year = tfile.Tag.Year.ToString();
                    LocalLibraryCollection.Add(new Track(title, filePath, duration, artists, album, year));
                }
            }
            else 
            {
                Directory.CreateDirectory(folderPath);
                //throw new DirectoryNotFoundException($"Local folder not found at \"{folderPath}\"");
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
            tfile.Tag.Year = uint.Parse(updatedTrack.Year);
            tfile.Save();
            await Task.Delay(500);
        }

    }
}
