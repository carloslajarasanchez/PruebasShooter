using UnityEngine;

public class PauseController : MonoBehaviour
{
    private IPauseService _pauseService;
    private PlayerInputActions _input;

    private void Awake()
    {
        _pauseService = AppContainer.Get<IPauseService>();
        _input = AppContainer.Get<IPlayerInput>().Actions;
    }

    private void OnEnable()
    {
        _input.Player.Pause.performed += OnPausePerformed;
        _input.UI.Pause.performed += OnPausePerformed;
    }

    private void OnDisable()
    {
        _input.Player.Pause.performed -= OnPausePerformed;
        _input.UI.Pause.performed -= OnPausePerformed;
    }

    private void OnPausePerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        => _pauseService.Toggle();
}
