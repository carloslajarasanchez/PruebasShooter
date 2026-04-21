using UnityEngine;

public abstract class Item : MonoBehaviour, ICatchable
{
    [SerializeField] private ItemData _data;

    public string Name { get; private set; }
    public string Description { get; private set; }
    public Sprite Icon { get; private set; }
    public GameObject ModelPrefab { get; private set; }

    private IInventoryService _inventoryService;

    protected void Awake()
    {
        Name = _data.ItemName;
        Description = _data.Description;
        Icon = _data.Icon;
        ModelPrefab = _data.ModelPrefab;
        _inventoryService = AppContainer.Get<IInventoryService>();
    }

    public void Catch()
    {
        _inventoryService.AddItem(this);
        Destroy(gameObject); // El GameObject del suelo se destruye; el modelo en Hand viene de ModelPrefab
    }

    /// <summary>
    /// Equipa este item en la mano del jugador a través de EquipService.
    /// Llamado desde el botón "Equipar" del inventario.
    /// </summary>
    public virtual void Equip()
    {
        AppContainer.Get<IEquipService>().Equip(this);
    }

    /// <summary>
    /// Uso directo desde inventario (no desde la mano). Las subclases lo sobreescriben.
    /// </summary>
    public virtual void Use() { }
}
