using UnityEngine;

public abstract class Item : MonoBehaviour, ICatchable
{
    [SerializeField] private ItemData _data;
    protected string _name;
    protected string _description;
    protected Sprite _icon;
    protected GameObject _modelPrefab;

    public string Name => _name;
    public string Description => _description;
    public Sprite Icon => _icon;
    public GameObject ModelPrefab => _modelPrefab;

    private IInventoryService _inventoryService;

    public virtual void Awake()
    {
        this._name = _data.ItemName;
        this._description = _data.Description;
        this._icon = _data.Icon;
        this._modelPrefab = _data.ModelPrefab;
        _inventoryService = AppContainer.Get<IInventoryService>();
    }

    public void Catch()
    {
        Debug.Log("Catching item: " + _name);
        _inventoryService.AddItem(this);
        //Destroy(gameObject);
        gameObject.SetActive(false);
        transform.SetParent(null);
    }

    public virtual void Use()
    {
        _inventoryService.RemoveItem(this);
    }

    public virtual void Equip()
    {
        Debug.Log("Equipping item: " + _name);
    }
}
