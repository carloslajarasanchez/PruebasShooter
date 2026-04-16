using System.Collections.Generic;
using UnityEngine;

public class InventoryService : IInventoryService
{
    public List<Item> Items { get; set; } = new List<Item>();
    private IEventService _eventService;
    private OnInventoryChanged _inventoryChangedEvent;

    public InventoryService()
    {
        _eventService = AppContainer.Get<IEventService>();
        _inventoryChangedEvent = new OnInventoryChanged();
    }

    public void AddItem(Item item)
    {
        //_eventService.Publish(GameEvents.OnInventoryChanged);
        _eventService.Publish(_inventoryChangedEvent);
        Debug.Log("Adding item: " + item.name);
        Items.Add(item);
    }

    public void RemoveItem(Item item)
    {
        //_eventService.Publish(GameEvents.OnInventoryChanged);
        _eventService.Publish(_inventoryChangedEvent);
        Debug.Log("Removing item: " + item.name); 
        Items.Remove(item);
    }
}
