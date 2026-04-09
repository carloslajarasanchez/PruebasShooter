using UnityEngine;

public class LogService : ILogService
{
    public void Add<T>(string message)
    {
        Debug.Log($"[{typeof(T).Name}] {message}");
    }
}
