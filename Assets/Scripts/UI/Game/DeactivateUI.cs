using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeactivateUI : MonoBehaviour
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
        _playerInput.Actions.Player.Pause.performed += OpenInventory;
        _playerInput.Actions.UI.Inventory.performed += CloseInventory;
        _playerInput.Actions.UI.Pause.performed += CloseInventory;
        _eventService.Subscribe<OnGameResumed>(OnPauseResumed);
        _eventService.Subscribe<OnGamePaused>(OnPaused);

    }

    private void OnPaused(OwnEventBase evnt)
    {
        _playerInput.Actions.Player.Inventory.performed -= OpenInventory;
        _playerInput.Actions.UI.Inventory.performed -= CloseInventory;
        OpenInventory(default);
    }

    private void OnPauseResumed(OwnEventBase evnt)
    {
        CloseInventory(default);
    }

    private void OnDisable()
    {
        _playerInput.Actions.Player.Inventory.performed -= OpenInventory;
        _playerInput.Actions.Player.Pause.performed -= OpenInventory;
        _eventService.Unsubscribe<OnGameResumed>(OnPauseResumed);
        _eventService.Unsubscribe<OnGamePaused>(OnPaused);
    }

    private void OnDestroy()
    {
        _playerInput.Actions.UI.Inventory.performed -= CloseInventory;
        _playerInput.Actions.UI.Pause.performed -= CloseInventory;
    }

    private void OpenInventory(InputAction.CallbackContext context)
    {
        gameObject.SetActive(false);
    }

    private void CloseInventory(InputAction.CallbackContext context)
    {
        gameObject.SetActive(true);
    }
}
