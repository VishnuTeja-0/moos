using System;
using System.IO;
using OpenTK.Audio.OpenAL;
using OpenTK.Audio;
using moos.Interfaces;
using NAudio.Wave;
using NLayer.NAudioSupport;

namespace moos.Services;

public class LinuxPlayerService : IAudioPlayer
{
    private int source;
    private int buffer;
    private ALFormat format;
    private int sampleRate;
    private byte[]? audioData;
    private AudioFileReader? audioFile;

    public void PlayTrack(string filePath)
    {
        try
        {
            source = AL.GenSource();
            buffer = AL.GenBuffer();
            
            (audioData, format, sampleRate) = DecodeMp3(filePath);
            
            AL.BufferData(buffer, format, audioData, sampleRate);
            AL.Source(source, ALSourcei.Buffer, buffer);
            
            AL.SourcePlay(source);
        }
        catch (Exception ex)
        {
            // Logging and error display
            Console.WriteLine(ex.Message);
        }
    }

    public void ResumeTrack()
    {
        AL.SourcePlay(source);
    }

    public void PauseTrack()
    {
        AL.SourcePause(source);
    }

    public void StopTrack()
    {
        AL.SourceStop(source);
        AL.Source(source, ALSourcei.Buffer, 0);
        AL.DeleteSource(source);
        AL.DeleteBuffer(buffer);
    }

    public void SetVolume(float newVolume)
    {
        if (audioData is not null)
        {
            float volume = newVolume / 100f;
            AL.Source(source, ALSourcef.Gain, volume);
        }
    }

    public void SeekToPosition(double seconds)
    {
        if (audioData is not null && audioFile is not null)
        {
            long newPosition = (long)(seconds * audioFile.WaveFormat.AverageBytesPerSecond);

            if((newPosition % audioFile.WaveFormat.BlockAlign) != 0)
            {
                newPosition -= newPosition % audioFile.WaveFormat.BlockAlign;
            }

            newPosition = Math.Clamp(newPosition, 0, audioFile.Length);
            
            AL.Source(source, ALSourcef.SecOffset, newPosition);
        }
    }

    public double GetPosition()
    {
        double time = 0;

        if (audioData is not null && audioFile is not null)
        {
            time = AL.GetSource(source, ALSourcef.SecOffset);
        }
        
        return time;
    }

    private (byte[], ALFormat, int) DecodeMp3(string filePath)
    {
        // var builder = new Mp3FileReader.FrameDecompressorBuilder(waveFormat => new Mp3FrameDecompressor(waveFormat));
        audioFile = new AudioFileReader(filePath);
        //using var pcmStream = WaveFormatConversionStream.CreatePcmStream(audioFile);
        using var memStream = new MemoryStream();
        
        audioFile.CopyTo(memStream);
        
        return (memStream.ToArray(), ALFormat.Stereo16, audioFile.WaveFormat.SampleRate);
    }
}