using UnityEngine;

public class EnemigoGordo : EnemigoBase
{
    [Header("Vision")]
    [SerializeField] private float _visionRange = 15f;
    [SerializeField] private float _visionAngle = 90f;
    [SerializeField] private float _visionHeight = 1.5f;

    [Header("Deteccion trasera")]
    [SerializeField] private float _rearDetectionRange = 3f;

    [Header("Linea de vision")]
    [SerializeField] private bool _checkLineOfSight = true;
    [SerializeField] private LayerMask _layerMaskObstacles = ~0;

    protected override void Update()
    {
        if (player == null) return;

        bool detected = IsPlayerInVisionCone(_visionRange, _visionAngle, _visionHeight, _checkLineOfSight, _layerMaskObstacles)
                     || IsPlayerInRearRange(_rearDetectionRange);

        HandleChaseState(detected);
    }

    private void LateUpdate()
    {
        _animator.SetFloat("speed", _agent.velocity.magnitude);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 forwardLeft = Quaternion.Euler(0, -_visionAngle * 0.5f, 0) * transform.forward * _visionRange;
        Vector3 forwardRight = Quaternion.Euler(0, _visionAngle * 0.5f, 0) * transform.forward * _visionRange;
        Gizmos.DrawLine(transform.position, transform.position + forwardLeft);
        Gizmos.DrawLine(transform.position, transform.position + forwardRight);

        // Rango de proximidad
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _rearDetectionRange);
    }
}
