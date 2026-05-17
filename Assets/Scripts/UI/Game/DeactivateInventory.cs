using UnityEngine;
using UnityEngine.InputSystem;

public class DeactivateInventory : MonoBehaviour
{
    private IPlayerInput _playerInput;
    private IEventService _eventService;

    private void Awake()
    {
        _playerInput = AppContainer.Get<IPlayerInput>();
        _eventService = AppContainer.Get<IEventService>();
    }

    private void OnEnable()
    {
        _playerInput.Actions.Player.Inventory.performed += OpenInventory;
        _playerInput.Actions.UI.Inventory.performed += CloseInventory;
        _eventService.Subscribe<OnGamePaused>(OnPaused);
    }

    private void OnDisable()
    {
        _playerInput.Actions.Player.Inventory.performed -= OpenInventory;
        _eventService.Unsubscribe<OnGamePaused>(OnPaused);
    }

    private void OnDestroy()
    {
        _playerInput.Actions.UI.Inventory.performed -= CloseInventory;
    }

    private void OpenInventory(InputAction.CallbackContext context)
    {
        gameObject.SetActive(false);
    }

    private void CloseInventory(InputAction.CallbackContext context)
    {
        gameObject.SetActive(true);
    }

    private void OnPaused(OwnEventBase e)
    {
        gameObject.SetActive(true);
    }
}

