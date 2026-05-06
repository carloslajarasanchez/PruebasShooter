using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public List<FlagEntry> flags;
    public List<FlagEntry> triggers;
    public List<ItemSaveEntry> items;
    public List<DoorSaveEntry> doors;
    public PlayerSaveEntry player;
}