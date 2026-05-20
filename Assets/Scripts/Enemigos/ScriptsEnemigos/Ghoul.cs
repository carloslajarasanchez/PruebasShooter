using UnityEngine;

// Enemigo de tipo Ghoul: misma mecánica de detección que EnemigoGordo
// (cono de visión frontal + detección trasera + sigilo al agacharse).
public class Ghoul : BaseEnemy
{
    [Header("Vision")]
    [SerializeField] private float _visionRange = 15f;
    [SerializeField] private float _visionAngle = 90f;
    [SerializeField] private float _visionHeight = 1.5f;

    [Header("Deteccion trasera")]
    [SerializeField] private float _rearDetectionRange = 3f;

    [Header("Sigilo")]
    [SerializeField] private float _crouchVisionRangeMultiplier = 0.5f;
    [SerializeField] private float _crouchVisionAngleMultiplier = 0.6f;

    [Header("Linea de vision")]
    [SerializeField] private bool _checkLineOfSight = true;
    [SerializeField] private LayerMask _layerMaskObstacles = ~0;

    [Header("Vision Original")]
    private float _originalVisionRange;
    private float _originalVisionAngle;

    protected override void Start()
    {
        base.Start();
        _originalVisionRange = _visionRange;
        _originalVisionAngle = _visionAngle;
    }

    protected override void Update()
    {
        if (player == null || _isDead) return;

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

        Gizmos.color = Color.green;
        Vector3 crouchLeft = Quaternion.Euler(0, -_visionAngle * 0.5f * _crouchVisionAngleMultiplier, 0) * transform.forward * _visionRange * _crouchVisionRangeMultiplier;
        Vector3 crouchRight = Quaternion.Euler(0, _visionAngle * 0.5f * _crouchVisionAngleMultiplier, 0) * transform.forward * _visionRange * _crouchVisionRangeMultiplier;
        Gizmos.DrawLine(transform.position, transform.position + crouchLeft);
        Gizmos.DrawLine(transform.position, transform.position + crouchRight);
        // Rango de proximidad
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _rearDetectionRange);
    }
    // Reduce el rango y ángulo de visión cuando el jugador está agachado,
    // aplicando los multiplicadores de sigilo configurados.
    protected override void HandlePlayerCrouch(OwnEventBase eventBase)
    {
        if (eventBase is OnPlayerCrouch crouchEvent)
        {
            _visionRange = crouchEvent.IsCrouching
                ? _originalVisionRange * _crouchVisionRangeMultiplier
                : _originalVisionRange;
            _visionAngle = crouchEvent.IsCrouching
                ? _originalVisionAngle * _crouchVisionAngleMultiplier
                : _originalVisionAngle;
        }
    }

}
