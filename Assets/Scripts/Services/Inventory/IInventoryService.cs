using System.Collections.Generic;

public interface IInventoryService
{
    public List<Item> Items { get; set; }
    public void AddItem(Item item);
    public void RemoveItem(Item item);
}
