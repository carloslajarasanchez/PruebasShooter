using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    private BaseEnemy _enemy;

    private void Awake()
    {
        _enemy = GetComponentInParent<BaseEnemy>();
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && _enemy != null && _enemy.CanDealDamage)
            AppContainer.Get<IPlayer>().RestLives(_enemy.GetDamage());
    }

    private void OnDrawGizmosSelected()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;
        Gizmos.color = Color.red;
        if (col is BoxCollider box)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else if (col is SphereCollider sphere)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(sphere.center, sphere.radius);
        }
        else if (col is CapsuleCollider)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(col.bounds.center - transform.position, Vector3.one * 0.3f);
        }
    }
}
