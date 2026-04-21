using UnityEngine;

public class EquipService : IEquipService
{
    public Item CurrentItem { get; private set; }
    public Item PreviousItem { get; private set; }

    private IEventService _eventService;
    private IInventoryService _inventoryService;
    private Transform _handTransform;
    private GameObject _currentModel;
    private int _itemLayer;

    public EquipService()
    {
        _eventService = AppContainer.Get<IEventService>();
        _inventoryService = AppContainer.Get<IInventoryService>();
        _itemLayer = LayerMask.NameToLayer("Item");
    }

    public void SetHandTransform(Transform hand) => _handTransform = hand;

    public void Equip(Item item)
    {
        if (item == null) return;
        if (item is not IEquippable)
        {
            Debug.LogWarning($"[EquipService] '{item.Name}' no implementa IEquippable.");
            return;
        }

        if (CurrentItem != null && CurrentItem != item)
            PreviousItem = CurrentItem;

        DestroyCurrentModel();
        CurrentItem = item;
        GameObject model = SpawnModel(item);

        // Si es un arma, le notificamos el modelo para que obtenga su AudioSource
        if (item is Weapon weapon && model != null)
            weapon.OnModelSpawned(model);

        _eventService.Publish(new OnItemEquipped { Item = item });
    }

    public void SwapWithPrevious()
    {
        if (PreviousItem == null) return;
        if (!_inventoryService.Items.Contains(PreviousItem))
        {
            PreviousItem = null;
            return;
        }
        Equip(PreviousItem);
    }

    public void Unequip()
    {
        if (CurrentItem == null) return;
        DestroyCurrentModel();
        _eventService.Publish(new OnItemUnequipped { Item = CurrentItem });
        CurrentItem = null;
    }

    public void UseCurrent()
    {
        if (CurrentItem == null) return;
        if (CurrentItem is not IEquippable equippable) return;

        equippable.OnPrimaryAction();
        _eventService.Publish(new OnItemUsed { Item = CurrentItem });

        if (!equippable.IsReusable)
        {
            _inventoryService.RemoveItem(CurrentItem);
            DestroyCurrentModel();
            Item next = PreviousItem;
            CurrentItem = null;
            if (next != null && _inventoryService.Items.Contains(next))
                Equip(next);
        }
    }

    private GameObject SpawnModel(Item item)
    {
        if (_handTransform == null)
        {
            Debug.LogError("[EquipService] Hand no asignado. Llama a SetHandTransform primero.");
            return null;
        }

        GameObject prefab = item.ModelPrefab;
        if (prefab == null)
        {
            Debug.LogWarning($"[EquipService] '{item.Name}' no tiene ModelPrefab en ItemData.");
            return null;
        }

        _currentModel = Object.Instantiate(prefab, _handTransform.position, _handTransform.rotation, _handTransform);

        if (_itemLayer != -1)
            SetLayerRecursively(_currentModel, _itemLayer);

        return _currentModel;
    }

    private void DestroyCurrentModel()
    {
        if (_currentModel != null)
        {
            Object.Destroy(_currentModel);
            _currentModel = null;
        }
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}
