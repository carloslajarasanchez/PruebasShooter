using UnityEngine;
using UnityEngine.InputSystem;

public class DeactivateInventory : MonoBehaviour
{
    private IPlayerInput _playerInput;

    private void Awake()
    {
        _playerInput = AppContainer.Get<IPlayerInput>();
    }

    private void OnEnable()
    {
        _playerInput.Actions.Player.Inventory.performed += OpenInventory;
        _playerInput.Actions.UI.Inventory.performed += CloseInventory;
    }

    private void OnDisable()
    {
        _playerInput.Actions.Player.Inventory.performed -= OpenInventory;
        //_playerInput.Actions.UI.Inventory.performed -= CloseInventory;
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

