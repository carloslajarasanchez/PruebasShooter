using UnityEngine;

public class PusheableObject : MonoBehaviour, IPusheable
{
    [SerializeField] private bool _canBePushed;
    private Rigidbody _rigidbody;
    public bool CanBePushed => _canBePushed;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public virtual void Push(Vector3 force)
    {
        if (_canBePushed)
        {
            _rigidbody.AddForce(force, ForceMode.Force);
            Debug.Log("This object can be pushed.");
        }
        else
        {
            Debug.Log("This object cannot be pushed.");
        }
    }

    public void MoveObject()
    {
        _canBePushed = true;
        Debug.Log("Door is now open and can be pushed.");
    }
    public void NotMoveObject() 
    {
        _canBePushed = false;
        Debug.Log("Door is now closed and cannot be pushed.");
    }
}