using System.Collections;
using UnityEngine;

public class SkullCap : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 5f;
    [SerializeField] private float _floatUpSpeed = 1f;
    [SerializeField] private float _lifeTime = 5f;

    private Rigidbody _rigidbody;
    private GameObject _sourcePrefab;
    private IPoolService _poolService;

    void Awake()
    {
       _rigidbody = GetComponent<Rigidbody>();
        _poolService = AppContainer.Get<IPoolService>();
    }

    private void OnEnable()
    {
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 1, Random.Range(0f, 1f)).normalized;
        _rigidbody.AddTorque(Vector3.up * _rotationSpeed, ForceMode.Impulse);
        _rigidbody.AddForce(randomDirection * _floatUpSpeed, ForceMode.Impulse);

        StartCoroutine(ReturnAfterLifeTime());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void Init(GameObject sourcePrefab)
    {
        _sourcePrefab = sourcePrefab;
    }

    private IEnumerator ReturnAfterLifeTime()
    {
        yield return new WaitForSeconds(_lifeTime);
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if(_sourcePrefab == null || _poolService == null)
        {
            gameObject.SetActive(false);
            return;
        }
        _poolService.Return(_sourcePrefab, gameObject);
    }
}
