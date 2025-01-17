using Avalonia.Controls.Chrome;
using Avalonia.Media.Imaging;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace moos.Models
{
    public class Track(string title, string filePath, TimeSpan duration,
        ObservableCollection<string> artists, string album = "", uint year = 0, string? lyrics = "",
        Bitmap? albumArt = null) : ICloneable
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
        public uint Year { get; set; } = year == 0 ? (uint) DateTime.Now.Year : year;

        public object Clone()
        {
            return new Track(Title, FilePath, Duration, Artists, Album, Year, Lyrics,
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
    }
}
