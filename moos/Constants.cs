using System;
using System.Configuration;
using System.IO;

namespace moos
{
    public static class Constants
    {
        public static readonly string LibraryFolder;
        public static readonly string PlaylistFolder;
        public static readonly string DefaultAlbumArtPath;
        public static readonly string ProjectDirectory;
        public static readonly string SoundTouchDllPath;
        public static readonly string AlbumIdSearchApi;
        public static readonly string TrackAlbumIdSearchApi;
        public static readonly string CoverArtApi;
        public static readonly float DefaultPlayingVolume;
        public static readonly float DefaultPlayingSpeed;
        public static readonly float DefaultPlayingPitch;
        public static readonly float DefaultCropPosition;
        public static readonly float DefaultCropSide;

        static Constants()
        {
            LibraryFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), ConfigurationManager.AppSettings["libraryFolder"] ?? "moos");
            PlaylistFolder = Path.Combine(LibraryFolder, ConfigurationManager.AppSettings["playlistFolder"] ?? "playlists");
            DefaultAlbumArtPath = ConfigurationManager.AppSettings["defaultAlbumArtPath"] ?? "";
            ProjectDirectory = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName ?? "";
            SoundTouchDllPath = Path.Combine(ProjectDirectory, ConfigurationManager.AppSettings["dllFolder"] ?? "lib");
            AlbumIdSearchApi = ConfigurationManager.AppSettings["albumIdSearchApi"] ?? "";
            TrackAlbumIdSearchApi = ConfigurationManager.AppSettings["trackAlbumIdSearchApi"] ?? "";
            CoverArtApi = ConfigurationManager.AppSettings["coverArtApi"] ?? "";
            DefaultPlayingVolume = float.Parse(ConfigurationManager.AppSettings["defaultPlayingVolume"] ?? "0");
            DefaultPlayingSpeed = float.Parse(ConfigurationManager.AppSettings["defaultPlayingSpeed"] ?? "0");
            DefaultPlayingPitch = float.Parse(ConfigurationManager.AppSettings["defaultPlayingPitch"] ?? "0");
            DefaultCropPosition = float.Parse(ConfigurationManager.AppSettings["defaultCropPosition"] ?? "25");
            DefaultCropSide = float.Parse(ConfigurationManager.AppSettings["defaultCropSide"] ?? "150");
        }

    }
}
