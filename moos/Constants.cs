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
        public static readonly string SoundTouchDllPath;
        public static readonly string AlbumIdSearchApi;
        public static readonly string CoverArtApi;
        public static readonly float DefaultPlayingVolume;
        public static readonly float DefaultPlayingSpeed;
        public static readonly float DefaultPlayingPitch;

        static Constants()
        {
            LibraryFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), ConfigurationManager.AppSettings["libraryFolder"] ?? "moos");
            DefaultAlbumArtPath = ConfigurationManager.AppSettings["defaultAlbumArtPath"] ?? "";
            ProjectDirectory = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName ?? "";
            SoundTouchDllPath = Path.Combine(ProjectDirectory, ConfigurationManager.AppSettings["dllFolder"] ?? "lib");
            AlbumIdSearchApi = ConfigurationManager.AppSettings["albumIdSearchApi"] ?? "";
            CoverArtApi = ConfigurationManager.AppSettings["coverArtApi"] ?? "";
            DefaultPlayingVolume = float.Parse(ConfigurationManager.AppSettings["defaultPlayingVolume"] ?? "0");
            DefaultPlayingSpeed = float.Parse(ConfigurationManager.AppSettings["defaultPlayingSpeed"] ?? "0");
            DefaultPlayingPitch = float.Parse(ConfigurationManager.AppSettings["defaultPlayingPitch"] ?? "0");
        }

    }
}
