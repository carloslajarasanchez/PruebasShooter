using UnityEngine;

public abstract class Item : MonoBehaviour, ICatchable
{
    [SerializeField] private ItemData _data;

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
    }

    public void Catch()
    {
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
    }

    public virtual void Equip()
    {
        _equipService.Equip(this);
    }

    public virtual void Use() { }
}