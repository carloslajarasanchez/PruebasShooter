using UnityEngine;
using UnityEngine.InputSystem;

public class SwitchLight : MonoBehaviour
{
    [SerializeField] private Light _flashlight;
    private IPlayerInput _playerInput;
    private ILogService _logService;
    private PlayerInputActions _actions;
    private bool _flashlightOn;

    private void Awake()
    {
        _playerInput = AppContainer.Get<IPlayerInput>();
    }

    private void OnFlashlight(InputAction.CallbackContext ctx)
    {
        _flashlightOn = !_flashlightOn;
        _flashlight.enabled = _flashlightOn;
    }

    private void OnEnable()
    {
        _actions = _playerInput.Actions;
        _actions.Player.SwitchLight.performed += OnFlashlight;
    }

    private void OnDisable()
    {
        _actions.Player.SwitchLight.performed -= OnFlashlight;
    }
}
