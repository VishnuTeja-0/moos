using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using TagLib;
using moos.Interfaces;


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
                    Bitmap? albumArt = null;
                    var pictures = tfile.Tag.Pictures;
                    if (pictures is not null && pictures.Length > 0)
                    {
                        using var stream = new MemoryStream(pictures[0].Data.Data);
                        albumArt = new Bitmap(stream);
                    }
                    TimeSpan duration = tfile.Properties.Duration;
                    ObservableCollection<string> artists = new ObservableCollection<string>(tfile.Tag.Performers);
                    string album = tfile.Tag.Album;
                    string year = tfile.Tag.Year.ToString();
                    LocalLibraryCollection.Add(new Track(title, filePath, duration, artists, album, year, albumArt));
                }
            }
            else 
            {
                Directory.CreateDirectory(folderPath);
                //throw new DirectoryNotFoundException($"Local folder not found at \"{folderPath}\"");
            }

            return LocalLibraryCollection;
        }

        public async void EditTrackMetadata(Track updatedTrack, string folderPath, IImageEditor imageEditor)
        {
            string filePath = updatedTrack.FilePath;

            var tfile = TagLib.File.Create(filePath);
            if (updatedTrack.AlbumArt is not null)
            {
                var jpegEncoded = imageEditor.EncodeToJpeg(updatedTrack.AlbumArt);
                using var memoryStream = new MemoryStream(jpegEncoded);
                memoryStream.Seek(0, SeekOrigin.Begin);
                var picture = new Picture
                {
                    Type = PictureType.FrontCover,
                    Description = "Cover",
                    MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg,
                    Data = ByteVector.FromStream(memoryStream)
                };
                tfile.Tag.Pictures = [picture];
            }
            tfile.Tag.Title = updatedTrack.Title;
            tfile.Tag.Performers = updatedTrack.Artists!.ToArray();
            tfile.Tag.Album = updatedTrack.Album;
            tfile.Tag.Year = uint.Parse(updatedTrack.Year);
            tfile.Save();
            await Task.Delay(500);
        }
    }
}
