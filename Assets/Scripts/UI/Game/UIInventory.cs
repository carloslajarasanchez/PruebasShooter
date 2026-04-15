using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIInventory : MonoBehaviour
{
    [SerializeField] private GameObject _inventoryPanel;
    [SerializeField] private GameObject _inventoryContainer;
    [SerializeField] private GameObject _inventoryItem;

    private IPlayerInput _playerInput;
    private IInventoryService _inventoryService;

    private void Awake()
    {
        _inventoryPanel.SetActive(false);
        _playerInput = AppContainer.Get<IPlayerInput>();
        _inventoryService = AppContainer.Get<IInventoryService>();
    }

    private void OnEnable()
    {
        _playerInput.Actions.Player.Inventory.performed += ActiveInventory;
        _playerInput.Actions.UI.Cancel.performed += DeactiveInventory;
    }

    private void OnDisable()
    {
        _playerInput.Actions.Player.Inventory.performed -= ActiveInventory;
        _playerInput.Actions.UI.Cancel.performed -= DeactiveInventory;
    }

    private void ActiveInventory(InputAction.CallbackContext context)
    {
        _inventoryPanel.SetActive(true);
        _playerInput.SwitchControlMap(ControlMap.UI);
        List<Item> playerInventory = _inventoryService.Items;

        if (playerInventory == null || playerInventory.Count == 0)
        {
            Debug.Log("Player inventory is empty.");
            return;
        }

        foreach (var item in playerInventory)
        {
            // Instantiate inventory item UI elements based on the player's inventory
            GameObject inventoryItemUI = Instantiate(_inventoryItem, _inventoryContainer.transform);
            inventoryItemUI.GetComponent<UIItem>().SetItem(item);
            // Set up the inventory item UI (e.g., set icon, name, etc.) based on the item data
        }
        // Populate inventory items here
        // For example, you can loop through the player's inventory and instantiate _inventoryItem for each item
    }

    private void DeactiveInventory(InputAction.CallbackContext context)
    {
        _inventoryPanel.SetActive(false);
        _playerInput.SwitchControlMap(ControlMap.Player);
        // Clear inventory items from the panel if needed
        foreach (Transform child in _inventoryContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
