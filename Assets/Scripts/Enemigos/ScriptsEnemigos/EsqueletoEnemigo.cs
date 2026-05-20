using UnityEngine;

// Enemigo tipo "Weeping Angel": se congela cuando el jugador lo mira (IsPlayerLookingAtMe)
// y persigue cuando no. Usa detección por caja (Physics.CheckBox) en lugar de cono de visión.
public class EsqueletoEnemigo : BaseEnemy
{
    [Header("Deteccion")]
    [SerializeField] private Vector3 _detectionBoxSize = new Vector3(8f, 3f, 8f);
    [SerializeField] private LayerMask _playerLayer;

    [SerializeField] private float _fieldOfViewAngle = 60f;
    [SerializeField] private bool _checkLineOfSight = true;
    [SerializeField] private LayerMask _layerMaskObstacles = ~0;

    [Header("Comportamiento")]
    [SerializeField] private float _lookAtSpeed = 0f;

    protected override void Update()
    {
        if (player == null)
            return;

        // Centro de la caja
        Vector3 boxCenter = transform.position + Vector3.up * (_detectionBoxSize.y * 0.5f);

        // Detectar jugador dentro del cubo
        bool playerInRange = Physics.CheckBox(
            boxCenter,
            _detectionBoxSize * 0.5f,
            Quaternion.identity,
            _playerLayer
        );

        // Si no esta dentro del rango
        if (!playerInRange)
        {
            _currentState = EnemyStateMachine.Idle;
            _agent.speed = 0f;
            CanDealDamage = false;

            if (_agent.hasPath)
                _agent.ResetPath();

            return;
        }

        // Si el jugador mira al enemigo
        if (IsPlayerLookingAtMe(
            _fieldOfViewAngle,
            _checkLineOfSight,
            _layerMaskObstacles))
        {
            _currentState = EnemyStateMachine.Idle;
            _agent.speed = _lookAtSpeed;
            CanDealDamage = false;

            if (_agent.hasPath)
                _agent.ResetPath();

            return;
        }

        // Perseguir jugador
        _currentState = EnemyStateMachine.Chasing;
        CanDealDamage = true;
        _agent.speed = _speed;
        _agent.destination = player.transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 boxCenter = transform.position + Vector3.up * (_detectionBoxSize.y * 0.5f);

        Gizmos.matrix = Matrix4x4.TRS(
            boxCenter,
            Quaternion.identity,
            Vector3.one
        );

        Gizmos.DrawWireCube(Vector3.zero, _detectionBoxSize);
    }
    // El esqueleto no tiene reacción de golpe procedural (se desintegra al morir directamente).
    public override void OnHitReaction(HumanBodyBones bone, Vector3 force, Rigidbody boneRb, float damage)
    {
        
    }
}