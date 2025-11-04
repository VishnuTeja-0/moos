
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using moos.Models;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;

namespace moos.Services
{
    public class YTDownloaderService
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
                ytdl = new YoutubeDL();

                ytdl.YoutubeDLPath = Path.Combine(dependencyPath, "yt-dlp");
                ytdl.FFmpegPath = Path.Combine(dependencyPath, "ffmpeg");
                ytdl.OutputFolder = folderPath;

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

        public void cancelCurrentDownload()
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

        public float GetProgressPercentage()
        {
            return progress * 100;
        }

        private static bool IsValidYoutubeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            return YouTubeUrlRegex.IsMatch(url);
        }
    }
}