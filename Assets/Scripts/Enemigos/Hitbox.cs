using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [SerializeField] private float _damageMultiplier = 1f;
    [SerializeField] private HumanBodyBones _bone;
    private BaseEnemy _enemy;
    private Rigidbody _boneRigidbody;

    private void Awake()
    {
        _enemy = GetComponentInParent<BaseEnemy>();
        _boneRigidbody = GetComponentInParent<Rigidbody>();
    }

    public void ReceiveDamage(float baseDamage, Vector3 hitForce)
    {
        if (_enemy == null) return;
        float finalDamage = baseDamage * _damageMultiplier;
        _enemy.TakeDamage(finalDamage);
        _enemy.OnHitReaction(_bone, hitForce, _boneRigidbody, finalDamage);
    }
}