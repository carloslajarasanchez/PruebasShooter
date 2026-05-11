using UnityEngine;

public class EsqueletoEnemigo : EnemigoBase
{
    [Header("Deteccion")]
    [SerializeField] private float _fieldOfViewAngle = 60f;
    [SerializeField] private bool _checkLineOfSight = true;
    [SerializeField] private LayerMask _layerMaskObstacles = ~0;

    [Header("Comportamiento")]
    [SerializeField] private float _lookAtSpeed = 0f;

    private bool _playerInRange;
    private BoxCollider _triggerCollider;

    private void Awake()
    {
        _triggerCollider = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            _playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            _playerInRange = false;
    }

    protected override void Update()
    {
        if (!_playerInRange || player == null)
        {
            _currentState = EnemyStateMachine.Idle;
            _agent.speed = 0f;
            if (_agent.hasPath)
                _agent.ResetPath();
            return;
        }

        if (IsPlayerLookingAtMe(_fieldOfViewAngle, _checkLineOfSight, _layerMaskObstacles))
        {
            _currentState = EnemyStateMachine.Idle;
            _agent.speed = _lookAtSpeed;
            if (_agent.hasPath)
                _agent.ResetPath();
        }
        else
        {
            _currentState = EnemyStateMachine.Chasing;
            _agent.speed = _speed;
            _agent.destination = player.transform.position;
        }
    }
}
