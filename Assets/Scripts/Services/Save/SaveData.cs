using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public List<FlagEntry> flags;
    public List<FlagEntry> triggers;
    public List<ItemSaveEntry> items;
    public List<DoorSaveEntry> doors;
    public List<EnemySaveEntry> enemies;
    public PlayerSaveEntry player;
    public string equippedItemId;
    public string previousItemId;
}

[System.Serializable]
public class EnemySaveEntry
{
    public string id;
    public EnemyState state;
}