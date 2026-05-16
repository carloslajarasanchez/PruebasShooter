using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIInventory : MonoBehaviour
{
    [SerializeField] private GameObject _inventoryPanel;
    [SerializeField] private GameObject _inventoryContainer;
    [SerializeField] private GameObject _inventoryItem;
    [SerializeField] private UIItemDetail _itemDetail;

    private IPlayerInput _playerInput;
    private IInventoryService _inventoryService;
    private IPauseService _pauseService;
    private IEventService _eventService;

    private UIItem _selectedUIItem;

    private void Awake()
    {
        _inventoryPanel.SetActive(false);
        _playerInput = AppContainer.Get<IPlayerInput>();
        _inventoryService = AppContainer.Get<IInventoryService>();
        _pauseService = AppContainer.Get<IPauseService>();
        _eventService = AppContainer.Get<IEventService>();
    }

    private void OnEnable()
    {
        _playerInput.Actions.Player.Inventory.performed += ActiveInventory;
        _playerInput.Actions.UI.Inventory.performed += DeactiveInventory;
        _eventService.Subscribe<OnGamePaused>(OnGamePaused);
    }

    private void OnDisable()
    {
        _playerInput.Actions.Player.Inventory.performed -= ActiveInventory;
        _playerInput.Actions.UI.Inventory.performed -= DeactiveInventory;
        _eventService.Unsubscribe<OnGamePaused>(OnGamePaused);
    }

    private void ActiveInventory(InputAction.CallbackContext context)
    {
        if (_pauseService.IsPaused) return;
        _inventoryPanel.SetActive(true);
        _playerInput.SwitchControlMap(ControlMap.UI);
        RefreshGrid();
    }

    private void DeactiveInventory(InputAction.CallbackContext context)
    {
        if (!_inventoryPanel.activeSelf) return;
        _inventoryPanel.SetActive(false);
        _playerInput.SwitchControlMap(ControlMap.Player);
        _itemDetail.Hide();
        _selectedUIItem = null;
        ClearGrid();
    }

    private void OnGamePaused(OwnEventBase e)
    {
        if (!_inventoryPanel.activeSelf) return;
        ForceCloseInventory();
    }

    private void ForceCloseInventory()
    {
        _inventoryPanel.SetActive(false);
        _itemDetail.Hide();
        _selectedUIItem = null;
        ClearGrid();
    }

    public void SelectItem(Item item)
    {
        // Desmarcar el slot anterior
        if (_selectedUIItem != null)
            _selectedUIItem.SetSelected(false);

        // Buscar el UIItem correspondiente al item seleccionado
        foreach (Transform child in _inventoryContainer.transform)
        {
            UIItem uiItem = child.GetComponent<UIItem>();
            if (uiItem != null)
            {
                // Lo marcamos como seleccionado si es el que se puls�
                // (UIItem guarda su propio _item, pero aqu� lo detectamos comparando)
            }
        }

        // Mostrar detalle en el panel derecho
        _itemDetail.ShowItem(item);
    }

    public void SetSelectedSlot(UIItem uiItem, Item item)
    {
        if (_selectedUIItem != null)
            _selectedUIItem.SetSelected(false);

        _selectedUIItem = uiItem;
        _selectedUIItem.SetSelected(true);
        _itemDetail.ShowItem(item);
    }

    private void RefreshGrid()
    {
        ClearGrid();
        List<Item> playerInventory = _inventoryService.Items;

        if (playerInventory == null || playerInventory.Count == 0)
        {
            Debug.Log("Inventario vac�o.");
            return;
        }

        foreach (Item item in playerInventory)
        {
            GameObject slotGO = Instantiate(_inventoryItem, _inventoryContainer.transform);
            UIItem uiItem = slotGO.GetComponent<UIItem>();
            uiItem.SetItem(item, this);
        }
    }

    private void ClearGrid()
    {
        foreach (Transform child in _inventoryContainer.transform)
            Destroy(child.gameObject);
    }
}
