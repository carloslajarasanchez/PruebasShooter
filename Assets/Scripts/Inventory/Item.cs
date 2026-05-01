using UnityEngine;
using UnityEngine.UIElements;

public abstract class Item : MonoBehaviour, ICatchable, ISavable<ItemState>
{
    [SerializeField] private ItemData _data;
    [SerializeField] private string saveId;
    protected bool _isInInventory;

    public string SaveId => saveId;
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Sprite Icon { get; private set; }
    public GameObject ModelPrefab { get; private set; }

    private IInventoryService _inventoryService;
    private IEquipService _equipService;

    protected void Awake()
    {
        Name = _data.ItemName;
        Description = _data.Description;
        Icon = _data.Icon;
        ModelPrefab = _data.ModelPrefab;
        _inventoryService = AppContainer.Get<IInventoryService>();
        _equipService = AppContainer.Get<IEquipService>();

    // Seguridad: si no tiene ID, se asigna uno en editor
        #if UNITY_EDITOR
                if (string.IsNullOrEmpty(saveId))
                {
                    saveId = System.Guid.NewGuid().ToString();
                    UnityEditor.EditorUtility.SetDirty(this);
                }
        #endif
    }

    public void Catch()
    {
        _isInInventory = true;
        // Desactivar collider y rigidbody para que no interfieran en el Player
        if (TryGetComponent<Collider>(out var col)) col.enabled = false;
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        // Reparentar al ItemStorage del player y desactivar
        Transform storage = _equipService.ItemStorage;
        if (storage != null)
        {
            transform.SetParent(storage);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        gameObject.SetActive(false);
        _inventoryService.AddItem(this);
        SaveState();
    }

    public virtual void Equip()
    {
        _equipService.Equip(this);
    }

    public virtual void Use() { }

    public virtual void SaveState(bool? isConsumed = null, int? currentAmmo = null)
    {
        var saveService = AppContainer.Get<ISaveService>();
        var state = saveService.GetItemState(SaveId) ?? new ItemState();

        state.isInInventory = _isInInventory;

        if (isConsumed.HasValue)
            state.isConsumed = isConsumed.Value;

        if (currentAmmo.HasValue)
            state.currentAmmo = currentAmmo.Value;

        saveService.SetItemState(SaveId, state);
    }

    public virtual void RestoreState(ItemState state)
    {
        _isInInventory = state.isInInventory;

        if (state.isConsumed)
        {
            Destroy(gameObject);
            return;
        }

        gameObject.SetActive(!_isInInventory);
        if(_isInInventory)
        {
            Catch();
        }
    }

}