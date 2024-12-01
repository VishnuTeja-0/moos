using Avalonia.Controls.Chrome;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace moos.Models
{
    public class Song
    {
        public string Title { get; set; }
        public string? Artist { get; set; }
        public string? Album { get; set; }
        public string FilePath { get; set; }
        public TimeSpan Duration { get; set; }
        public string? Lyrics { get; set; }
        public Bitmap? AlbumArt { get; set; }
        public string? Year { get; set; }

        public Song(string title, string filePath, TimeSpan duration,
            string? artist = "", string? album = "", string? lyrics = "",
            Bitmap? albumArt = null, string? year = "") 
        {
            Title = title;
            FilePath = filePath;
            Duration = duration;
            Artist = artist;
            Album = album;
            Lyrics = lyrics;
            AlbumArt = albumArt;
            Year = year;
        }

    }
}
