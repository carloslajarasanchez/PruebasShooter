public interface IPauseService
{
    bool IsPaused { get; }
    void Pause();
    void Resume();
    void Toggle();
}