using UnityEngine;

public interface IPoolService 
{
    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation);
    public void Return(GameObject prefab, GameObject instance);
}
