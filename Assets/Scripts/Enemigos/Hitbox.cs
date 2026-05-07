using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [SerializeField] private float _damageMultiplier = 1f;

    private EnemigoBase _enemy;

    private void Awake()
    {
        _enemy = GetComponentInParent<EnemigoBase>();
    }

    public void ReceiveDamage(float baseDamage)
    {
        if (_enemy != null)
            _enemy.TakeDamage(baseDamage * _damageMultiplier);
    }
}
