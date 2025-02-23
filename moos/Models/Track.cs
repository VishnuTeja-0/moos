using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace moos.Models
{
    public class Track(string title, string filePath, TimeSpan duration,
        ObservableCollection<string>? artists, string album = "", string year = "", string? lyrics = "",
        Bitmap? albumArt = null) : ICloneable, IEquatable<Track>
    {
        public string Title { get; set; } = title;
        public ObservableCollection<string>? Artists { get; set; } = artists ?? ([]);
        public string DisplayArtists
        {
            get
            {
                return Artists is not null ? string.Join(", ", Artists) : "";
            }
        }
        public string? Album { get; set; } = album;
        public string FilePath { get; set; } = filePath;
        public TimeSpan Duration { get; set; } = duration;
        public string DisplayDuration
        {
            get
            {
                return Duration.ToString("mm\\:ss");
            }
        }
        public string? Lyrics { get; set; } = lyrics;
        public Bitmap? AlbumArt { get; set; } = albumArt;
        public MemoryStream? DisplayAlbumArt
        {
            get
            {
                if(AlbumArt is not null)
                {
                    var memoryStream = new MemoryStream();
                    AlbumArt.Save(memoryStream);
                    memoryStream.Position = 0;
                    return memoryStream;
                }
                else
                {
                    return null;
                }
            }
        }
        public string Year { get; set; } = year == "" ? DateTime.Now.Year.ToString() : year;

        public object Clone()
        {
            ObservableCollection<string> clonedArtists = [];
            if (Artists is not null && Artists.Count > 0)
            {
                foreach (var item in (IEnumerable)Artists)
                {
                    ICloneable? cloneable = item as ICloneable;
                    if (cloneable is not null)
                    {
                        clonedArtists.Add((string)cloneable.Clone());
                    }
                }
            }

            return new Track(Title, FilePath, Duration, clonedArtists, Album, Year, Lyrics,
                AlbumArt is null ? null : CopyBitmap(AlbumArt));
        }

        public void SetAlbumArt(string filePath)
        {
            this.AlbumArt = new Bitmap(AssetLoader.Open(new Uri(filePath)));
        }

        private static Bitmap CopyBitmap(Bitmap original)
        {
            using (var memoryStream = new MemoryStream())
            {
                original.Save(memoryStream);
                memoryStream.Position = 0;

                return new Bitmap(memoryStream);
            }
        }

        public bool Equals(Track? other)
        {
            if (
                other is null ||
                this.Title.Trim() != other.Title.Trim() || 
                this.Year != other.Year ||
                this.Album != other.Album ||
                this.Artists!.Except(other.Artists!).Any() ||
                other.Artists!.Except(this.Artists!).Any()
                )
            {
                return false;
            }

            return true;
        }
    }
}
