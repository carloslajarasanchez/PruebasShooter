using UnityEngine;

public class PauseService : IPauseService
{
    public bool IsPaused { get; private set; }
    public bool IsPauseBlocked { get; set; }

    private IEventService _eventService;

    public PauseService()
    {
        _eventService = AppContainer.Get<IEventService>();
    }

    //ScriptableObject para el prefab de la UI que sea el pausePrefab, al servicio al crearlo le pasamos ese scriptable objet que esta en la carpeta resources con Resoruces.Load(Ese scriptableObject) y en el program le pasas eso (Servicio necesitan clases de configuracion que en este caso es un scriptableObject)
    public void Pause()
    {
        if (IsPaused) return;
        if (IsPauseBlocked) return;
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