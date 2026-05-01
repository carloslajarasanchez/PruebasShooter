public interface ISavable<T>
{
    string SaveId { get; }
    void RestoreState(T state);
}