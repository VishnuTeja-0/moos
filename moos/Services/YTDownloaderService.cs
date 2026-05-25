using System.Text.RegularExpressions;
using moos.Interfaces.Services;
using moos.Models;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;

namespace moos.Services
{
    public class YTDownloaderService : IYtDownloader
    {
        private static readonly Regex YouTubeUrlRegex = new Regex(
        @"^(?:https?:\/\/)?(?:www\.)?(?:youtube\.com\/(?:watch\?v=|embed\/|v\/|shorts\/)|youtu\.be\/)([a-zA-Z0-9_-]{11})",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private YoutubeDL? ytdl;
        private CancellationTokenSource? cts;
        private float progress = 5;
        
        public async Task<(bool, string)> DownloadSong(string url, string folderPath, string dependencyPath)
        {
            bool isSuccess = false;
            string downloadResult = "";

            if(IsValidYoutubeUrl(url)){
                var ytdl = GetYTDLInstance(folderPath, dependencyPath);
                var progress = new Progress<DownloadProgress>(p =>
                {
                    this.progress = p.Progress;
                    //DownloadProgressChanged?.Invoke(p.Progress);
                });
                cts = new CancellationTokenSource();
                var res = await ytdl.RunAudioDownload(url: url, 
                                                        format: YoutubeDLSharp.Options.AudioConversionFormat.Mp3,
                                                        progress: progress, ct: cts.Token);

                isSuccess = res.Success;
                downloadResult = res.Success ? res.Data : string.Join(",", res.ErrorOutput);
            }

            return (isSuccess, downloadResult);
        }

        public void CancelCurrentDownload()
        {
            if(cts is not null){
                cts.Cancel();
            }
        }

        public async Task<(bool, Track?)> FetchVideoMetadata(string url, Track downloadedTrack)
        {
            var res = await ytdl.RunVideoDataFetch(url);
            if (res.Success)
            {
                VideoData video = res.Data;
                
                downloadedTrack.Title = video.Title;
                downloadedTrack.Year = video.ReleaseYear;
                downloadedTrack.Artists = [video.Artist];
                downloadedTrack.Album = video.Album;
            }

            return (res.Success, downloadedTrack);
        }

        public float GetDownloadProgressPercentage()
        {
            return progress * 100;
        }

        private static bool IsValidYoutubeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            return YouTubeUrlRegex.IsMatch(url);
        }

        public async Task<(bool,List<Track>)> GetSearchResults(string searchString, string folderPath, string dependencyPath)
        {
            List<Track> searchResults = [];
            
            var ytdl = GetYTDLInstance(folderPath, dependencyPath);
            var options = new OptionSet()
            {
                DefaultSearch = "ytsearch:" + searchString
            };
            cts = new CancellationTokenSource();
            var res = await ytdl.RunWithOptions("", options, cts.Token);

            var isSuccess = res.Success;
            
            
            return (false, searchResults);
        }

        private YoutubeDL GetYTDLInstance(string folderPath, string dependencyPath)
        {
            ytdl = new YoutubeDL();

            ytdl.YoutubeDLPath = Path.Combine(dependencyPath, "yt-dlp");
            ytdl.FFmpegPath = Path.Combine(dependencyPath, "ffmpeg");
            ytdl.OutputFolder = folderPath;

            return ytdl;
        }
    }
}