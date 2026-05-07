using UnityEngine;

public class EsqueletoEnemigo : EnemigoBase
{
    [Header("Deteccion")]
    [SerializeField] private float _detectionRange = 10f;
    [SerializeField] private float _fieldOfViewAngle = 60f;
    [SerializeField] private bool _checkLineOfSight = true;
    [SerializeField] private LayerMask _layerMaskObstacles = ~0;

    [Header("Comportamiento")]
    [SerializeField] private float _lookAtSpeed = 0f;

    private Transform _cameraTransform;
    private bool _playerInRange;
    private BoxCollider _triggerCollider;

    private void Awake()
    {
        _triggerCollider = GetComponent<BoxCollider>();
        if (_triggerCollider == null)
            _triggerCollider = gameObject.AddComponent<BoxCollider>();

        _triggerCollider.isTrigger = true;
        _triggerCollider.size = new Vector3(_detectionRange * 2, 1.5f, _detectionRange * 2);
    }

    protected override void Start()
    {
        base.Start();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (Camera.main != null)
            _cameraTransform = Camera.main.transform;
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

        bool playerLookingAtMe = IsPlayerLookingAtMe();

        if (playerLookingAtMe)
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

    private bool IsPlayerLookingAtMe()
    {
        if (_cameraTransform == null)
            return false;

        Vector3 directionToEnemy = (transform.position - _cameraTransform.position).normalized;
        float angle = Vector3.Angle(_cameraTransform.forward, directionToEnemy);

        if (angle > _fieldOfViewAngle * 0.5f)
            return false;

        if (_checkLineOfSight)
        {
            Vector3 direction = transform.position - _cameraTransform.position;
            float distance = direction.magnitude;

            if (Physics.Raycast(_cameraTransform.position, direction.normalized, out RaycastHit hit, distance, _layerMaskObstacles))
            {
                if (!hit.collider.TryGetComponent<EsqueletoEnemigo>(out _))
                    return false;
            }
        }

        return true;
    }
}
