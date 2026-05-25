using moos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace moos.Interfaces.Services;
public interface IYtDownloader
{
    Task<(bool, string)> DownloadSong(string url, string folderPath, string dependencyPath);
    Task<(bool, Track?)> FetchVideoMetadata(string url, Track downloadedTrack);
    void CancelCurrentDownload();
    float GetDownloadProgressPercentage();
    Task<(bool,List<Track>)> GetSearchResults(string searchString, string folderPath, string dependencyPath);
}   

