using System;
using System.Runtime.InteropServices;
using moos.Interfaces;

namespace moos.Services;

public static class AudioPlayerFactory
{
    public static IAudioPlayer CreatePlayer()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new PlayerService();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return new LinuxPlayerService();
        }
        else
        {
            throw new PlatformNotSupportedException("The current platform is not supported.");
        }
    }
}