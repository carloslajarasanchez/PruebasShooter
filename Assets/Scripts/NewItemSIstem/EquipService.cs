using UnityEngine;

public class EquipService : IEquipService
{
    public Item CurrentItem { get; private set; }
    public Item PreviousItem { get; private set; }
    public Transform ItemStorage { get; private set; }
    public Transform Hand { get; private set; }

    private IEventService _eventService;
    private IInventoryService _inventoryService;
    private ILogService _logService;
    private int _handLayer;
    private int _defaultLayer;

    public EquipService()
    {
        _eventService = AppContainer.Get<IEventService>();
        _inventoryService = AppContainer.Get<IInventoryService>();
        _logService = AppContainer.Get<ILogService>();
        _handLayer = LayerMask.NameToLayer("Hand");
        _defaultLayer = LayerMask.NameToLayer("Default");

        if (_handLayer == -1)
            _logService.Add<EquipService>("Layer 'Hand' no existe. Créalo en Project Settings > Tags and Layers.");
    }

    /// <summary>
    /// Llamado desde EquipController.Awake() para inyectar los transforms del Player.
    /// </summary>
    public void SetTransforms(Transform itemStorage, Transform hand)
    {
        ItemStorage = itemStorage;
        Hand = hand;
    }

    // ?? API pública ??????????????????????????????????????????????

    public void Equip(Item item)
    {
        if (item == null) return;
        if (item is not IEquippable)
        {
            _logService.Add<EquipService>($"'{item.Name}' no implementa IEquippable. No se puede equipar.");
            return;
        }

        // Guardar anterior
        if (CurrentItem != null && CurrentItem != item)
            PreviousItem = CurrentItem;

        // Desequipar el actual sin perder su referencia aún
        UnequipCurrent();

        // Mover el nuevo item de ItemStorage a Hand
        CurrentItem = item;
        MoveToHand(item);

        // Si es arma notificar el AudioSource
        if (item is Weapon weapon)
            weapon.OnEquipped();

        _eventService.Publish(new OnItemEquipped { Item = item });
        _logService.Add<EquipService>($"Equipado: '{item.Name}'.");
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
        UnequipCurrent();
        _eventService.Publish(new OnItemUnequipped { Item = CurrentItem });
        CurrentItem = null;
    }

    public void UseCurrent()
    {
        if (CurrentItem == null) return;
        if (CurrentItem is not IEquippable equippable) return;

        equippable.OnPrimaryAction();
        _eventService.Publish(new OnItemUsed { Item = CurrentItem });

        // Si es consumible, eliminar del inventario y volver al anterior
        if (!equippable.IsReusable)
        {
            Item consumed = CurrentItem;
            CurrentItem = null;

            MoveToStorage(consumed);
            Object.Destroy(consumed.gameObject);
            _inventoryService.RemoveItem(consumed);

            if (PreviousItem != null && _inventoryService.Items.Contains(PreviousItem))
                Equip(PreviousItem);
        }
    }

    // ?? Movimiento entre transforms ??????????????????????????????

    private void MoveToHand(Item item)
    {
        if (Hand == null)
        {
            _logService.Add<EquipService>("Hand no asignado.");
            return;
        }

        item.transform.SetParent(Hand);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        item.gameObject.SetActive(true);
        SetLayerRecursively(item.gameObject, _handLayer);
    }

    private void UnequipCurrent()
    {
        if (CurrentItem == null) return;
        MoveToStorage(CurrentItem);
    }

    private void MoveToStorage(Item item)
    {
        if (ItemStorage == null) return;

        _logService.Add<EquipService>($"Desequipando '{item.Name}' a ItemStorage.");

        item.gameObject.SetActive(false);
        item.transform.SetParent(ItemStorage);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        SetLayerRecursively(item.gameObject, _defaultLayer);
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        if (layer == -1) return;
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}