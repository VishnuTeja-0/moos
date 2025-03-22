using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace moos
{
    public static class Constants
    {
        public static readonly string LibraryFolder;
        public static readonly string DefaultAlbumArtPath;
        public static readonly string ProjectDirectory;
        public static readonly float DefaultPlayingVolume;
        public static readonly double DefaultPlayingSpeed;
        public static readonly double DefaultPlayingPitch;

        static Constants()
        {
            LibraryFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), ConfigurationManager.AppSettings["libraryFolder"] ?? "moos");
            DefaultAlbumArtPath = ConfigurationManager.AppSettings["defaultAlbumArtPath"] ?? "";
            ProjectDirectory = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName ?? "";
            DefaultPlayingVolume = float.Parse(ConfigurationManager.AppSettings["defaultPlayingVolume"] ?? "0");
            DefaultPlayingSpeed = Double.Parse(ConfigurationManager.AppSettings["defaultPlayingSpeed"] ?? "0");
            DefaultPlayingPitch = Double.Parse(ConfigurationManager.AppSettings["defaultPlayingPitch"] ?? "0");
        }

    }
}
