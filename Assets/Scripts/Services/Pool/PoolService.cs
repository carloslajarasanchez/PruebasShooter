using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;

public class PoolService : IPoolService
{
    private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new Dictionary<GameObject, Queue<GameObject>>();
    private readonly Dictionary<GameObject, Transform> _containers = new Dictionary<GameObject, Transform>();

    private readonly Transform _root;
    private ILogService _logService;

    public PoolService()
    {
        GameObject root = new GameObject("[Pool]");
        Object.DontDestroyOnLoad(root);
        _root = root.transform;
        _logService = AppContainer.Get<ILogService>();
    }
    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
        {
            _logService.Add<PoolService>("Prefab is null");
            return null;
        }

        EnsurePoolExists(prefab);

        Queue<GameObject> queue = _pools[prefab];
        GameObject instance = queue.Count > 0 ? queue.Dequeue() : CreateInstance(prefab);

        instance.transform.SetPositionAndRotation(position, rotation);
        instance.SetActive(true);
        return instance;
    }

    public void Return(GameObject prefab, GameObject instance)
    {
        if (prefab == null || instance == null)
        {
            _logService.Add<PoolService>("Prefab or instance is null");
            return;
        }

        EnsurePoolExists(prefab);

        instance.SetActive(false);
        instance.transform.SetParent(_containers[prefab]);
        _pools[prefab].Enqueue(instance);
    }

    private void EnsurePoolExists(GameObject prefab)
    {
        if (_pools.ContainsKey(prefab)) return;

        GameObject container = new GameObject($"[Pool]{prefab.name}");
        container.transform.SetParent(_root);

        _pools[prefab] = new Queue<GameObject>();
        _containers[prefab] = container.transform;
    }

    private GameObject CreateInstance(GameObject prefab)
    {
        GameObject instance = Object.Instantiate(prefab, _containers[prefab]);
        instance.SetActive(false);
        return instance;
    }

}
