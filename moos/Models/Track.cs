using Avalonia.Controls.Chrome;
using Avalonia.Media.Imaging;
using DynamicData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace moos.Models
{
    public class Track(string title, string filePath, TimeSpan duration,
        ObservableCollection<string> artists, string album = "", string year = "", string? lyrics = "",
        Bitmap? albumArt = null) : ICloneable, IEquatable<Track>
    {
        public string Title { get; set; } = title;
        public ObservableCollection<string>? Artists { get; set; } = artists ?? ([]);
        public string DisplayArtists
        {
            get
            {
                return Artists != null ? string.Join(", ", Artists) : "";
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
        public string Year { get; set; } = year == "" ? DateTime.Now.Year.ToString() : year;

        public object Clone()
        {
            ObservableCollection<string> clonedArtists = [];
            if (artists != null && artists.Count > 0)
            {
                foreach (var item in (IEnumerable)Artists)
                {
                    ICloneable? cloneable = item as ICloneable;
                    if (cloneable != null)
                    {
                        clonedArtists.Add((string)cloneable.Clone());
                    }
                }
            }

            return new Track(Title, FilePath, Duration, clonedArtists, Album, Year, Lyrics,
                AlbumArt == null ? null : CopyBitmap(AlbumArt));
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
                other == null ||
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
