public interface ISaveService
{

    // SAVE / LOAD

    void Save();
    void Load();
    void RestoreScene();

    // STATE API

    T GetOrCreateState<T>(string id) where T : new();

    bool TryGetState<T>(string id, out T state);

    void SetState<T>(string id, T state);

}