using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

public class DependencyService
{
    private readonly string _projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
    private string _ytdlpFile;
    private string _ffmpegFile;

    public async Task LoadAppDependencies()
    {
        SetDependencyNames();
        string ytDlpPath = Path.Combine(_projectDirectory, _ytdlpFile);
        string ffmpegPath = Path.Combine(_projectDirectory, _ffmpegFile);

        if(!File.Exists(ytDlpPath))
        {
            await YoutubeDLSharp.Utils.DownloadYtDlp(_projectDirectory);
        }
        if(!File.Exists(ffmpegPath))
        {
            await YoutubeDLSharp.Utils.DownloadFFmpeg(_projectDirectory);
        }

        if(OperatingSystem.IsLinux()){
            SetExecutablePermission(ytDlpPath);
            SetExecutablePermission(ffmpegPath);
        }
        
    }

    private void SetDependencyNames()
    {
        if (OperatingSystem.IsWindows())
        {
            _ytdlpFile = "yt-dlp.exe";
            _ffmpegFile = "ffmpeg.exe";
        }
        else if(OperatingSystem.IsLinux())
        {
            _ytdlpFile = "yt-dlp";
            _ffmpegFile = "ffmpeg";
        }
    }

    private void SetExecutablePermission(string filePath)
    {
        var chmodProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/chmod",
                Arguments = $"+x \"{filePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        chmodProcess.Start();
        chmodProcess.WaitForExit();
    }
}