namespace moos.Interfaces;

public interface IAudioPlayer
{
    void PlayTrack(string filePath);
    void ResumeTrack();
    void PauseTrack();
    void StopTrack();
    void SetVolume(float volume);
    void SeekToPosition(double seconds);
    double GetPosition();
}