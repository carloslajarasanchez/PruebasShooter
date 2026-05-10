public interface IAudioService
{
    void Play(SoundType type);
    void PlayLoop(SoundType type);
    void PlayWithRandomPitch(SoundType type);
    void Stop(SoundType type);
    void Pause(SoundType type);
    void Resume(SoundType type);
    void StopAll();
}