using System.Runtime.InteropServices;
using moos.Interfaces.Services;
using NAudio.Wave.Alsa;

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
            return new PlayerService();
            //return new SoundFlowPlayerService();
            //return new OpenALPlayerService();
        }
        else
        {
            throw new PlatformNotSupportedException("The current platform is not supported.");
        }
    }
}