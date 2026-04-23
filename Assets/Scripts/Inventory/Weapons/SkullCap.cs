using UnityEngine;

public class SkullCap : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 5f;
    [SerializeField] private float _floatUpSpeed = 1f;
    private Rigidbody _rigidbody;

    void Awake()
    {
       _rigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 1, Random.Range(0f, 1f)).normalized;
        _rigidbody.AddTorque(Vector3.up * _rotationSpeed, ForceMode.Impulse);
        _rigidbody.AddForce(randomDirection * _floatUpSpeed, ForceMode.Impulse);
        Destroy(gameObject, 5f);
    }
}
