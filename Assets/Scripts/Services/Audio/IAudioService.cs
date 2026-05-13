public interface IAudioService
{
    void PlayBackgroundMusic(SoundType type);
    void Play(SoundType type);
    void PlayLoop(SoundType type);
    void PlayWithRandomPitch(SoundType type);
    void Stop(SoundType type);
    void Pause(SoundType type);
    void Resume(SoundType type);
    void StopAll();

    bool IsPlaying(SoundType type);
}