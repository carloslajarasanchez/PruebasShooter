public class OnItemEquipped : OwnEventBase
{
    public Item Item { get; set; }
}

public class OnItemUnequipped : OwnEventBase
{
    public Item Item { get; set; }
}

public class OnItemUsed : OwnEventBase
{
    public Item Item { get; set; }
}
