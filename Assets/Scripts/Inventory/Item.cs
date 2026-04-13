using UnityEngine;

public abstract class Item : MonoBehaviour, ICatchable
{
    [SerializeField] private ItemData _data;
    protected string _name;
    protected string _description;
    protected Sprite _icon;

    private void Awake()
    {
        this._name = _data.ItemName;
        this._description = _data.Description;
        this._icon = _data.Icon;
    }

    public void Catch()
    {
        Debug.Log("Catching item: " + _name);
        AppContainer.Get<IPlayer>().AddItem(this);
        Destroy(gameObject);
    }

    public virtual void Use()
    {
        AppContainer.Get<IPlayer>().RemoveItem(this);
    }

    public virtual void Equip()
    {
        Debug.Log("Equipping item: " + _name);
    }
}
