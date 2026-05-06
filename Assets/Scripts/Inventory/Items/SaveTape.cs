public class SaveTape : Item
{
    public void Consume()
    {
        SaveState(isConsumed: true);
        _inventoryService.RemoveItem(this);
        Destroy(gameObject);
    }
}
