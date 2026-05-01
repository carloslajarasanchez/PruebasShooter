public interface ISaveService
{
    void Save();
    void Load();

    ItemState GetItemState(string id);
    void SetItemState(string id, ItemState state);
}