using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

public class DependencyService
{
    private readonly string _projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;

    public async Task LoadAppDependencies()
    {
        string ytDlpPath = Path.Combine(_projectDirectory, "yt-dlp");
        string ffmpegPath = Path.Combine(_projectDirectory, "ffmpeg");

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