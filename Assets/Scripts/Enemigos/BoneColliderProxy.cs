using UnityEngine;

public class BoneColliderProxy : MonoBehaviour
{
    [SerializeField] private Transform _boneToFollow;

    private void LateUpdate()
    {
        transform.position = _boneToFollow.position;
        transform.rotation = _boneToFollow.rotation;
    }
}