public interface IPauseService
{
    bool IsPaused { get; }
    bool IsPauseBlocked { get; set; }
    void Pause();
    void Resume();
    void Toggle();
}