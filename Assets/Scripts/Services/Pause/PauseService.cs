using UnityEngine;

public class PauseService : IPauseService
{
    public bool IsPaused { get; private set; }

    private IEventService _eventService;

    public PauseService()
    {
        _eventService = AppContainer.Get<IEventService>();
    }

    public void Pause()
    {
        if (IsPaused) return;
        IsPaused = true;
        Time.timeScale = 0f;
        _eventService.Publish(new OnGamePaused());
    }

    public void Resume()
    {
        if (!IsPaused) return;
        IsPaused = false;
        Time.timeScale = 1f;
        _eventService.Publish(new OnGameResumed());
    }

    public void Toggle()
    {
        if (IsPaused) Resume();
        else Pause();
    }
}