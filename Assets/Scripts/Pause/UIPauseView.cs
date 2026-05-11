using UnityEngine;

public class UIPauseView : MonoBehaviour
{
    [SerializeField] private GameObject _panelContainer;

    private IPauseService _pauseService;
    private IEventService _eventService;
    private IPlayerInput _playerInput;

    private void Awake()
    {
        _pauseService = AppContainer.Get<IPauseService>();
        _eventService = AppContainer.Get<IEventService>();
        _playerInput = AppContainer.Get<IPlayerInput>();

        _panelContainer.SetActive(false);
    }

    private void OnEnable()
    {
        _eventService.Subscribe<OnGamePaused>(OnPaused);
        _eventService.Subscribe<OnGameResumed>(OnResumed);
    }

    private void OnDisable()
    {
        _eventService.Unsubscribe<OnGamePaused>(OnPaused);
        _eventService.Unsubscribe<OnGameResumed>(OnResumed);
    }

    private void OnPaused(OwnEventBase e)
    {
        _panelContainer.SetActive(true);
        //_playerInput.DisablePlayer(); // bloquea el input del juego
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnResumed(OwnEventBase e)
    {
        _panelContainer.SetActive(false);
        //_playerInput.EnablePlayer(); // reactiva el input del juego
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Enlaza este método al botón Continuar en el Inspector
    public void OnContinueClicked() => _pauseService.Resume();

    // Enlaza este método al botón Salir en el Inspector
    public void OnQuitClicked() => Application.Quit();
}