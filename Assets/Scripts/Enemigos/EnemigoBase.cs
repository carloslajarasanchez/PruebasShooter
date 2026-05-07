using UnityEngine;
using UnityEngine.AI;

public class EnemigoBase : MonoBehaviour, ISavable<EnemyState>
{
    [SerializeField] private float _life;
    [SerializeField] private float _damage;
    [SerializeField] protected float _speed;
    protected EnemyStateMachine _currentState;
    private string _saveId;
    protected NavMeshAgent _agent;
    public GameObject player;
    protected Transform _cameraTransform;

    [Header("Patrullaje")]
    [SerializeField] private Transform[] _patrolPoints;
    [SerializeField] private float _patrolWaitTime = 2f;
    [SerializeField] private float _patrolStopDistance = 1f;

    [Header("Persecucion")]
    [SerializeField] private float _loseSightTime = 2f;

    private int _currentPatrolIndex;
    private float _patrolWaitTimer;
    private float _loseSightTimer;
    protected bool _isChasing;

    public string SaveId => _saveId;

    protected virtual void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = _speed;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (Camera.main != null)
            _cameraTransform = Camera.main.transform;
    }

    protected virtual void Update()
    {
    }

    public virtual void TakeDamage(float damage)
    {
        _life -= damage;
        if (_life <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    // ── Helpers de deteccion ──────────────────────────────────────

    protected bool IsPlayerInVisionCone(float range, float angle, float height, bool checkLOS, LayerMask obstacles)
    {
        if (player == null) return false;

        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance > range)
            return false;

        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > angle * 0.5f)
            return false;

        if (checkLOS)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * height;

            if (Physics.Raycast(rayOrigin, directionToPlayer, out RaycastHit hit, distance, obstacles))
            {
                if (!hit.collider.TryGetComponent<EnemigoBase>(out _) && !hit.collider.CompareTag("Player"))
                    return false;
            }
        }

        return true;
    }

    protected bool IsPlayerInRearRange(float range)
    {
        if (player == null) return false;

        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.transform.position);
        float dot = Vector3.Dot(transform.forward, directionToPlayer);

        return dot < 0f && distance <= range;
    }

    protected bool IsPlayerLookingAtMe(float fovAngle, bool checkLOS, LayerMask obstacles)
    {
        if (_cameraTransform == null || player == null)
            return false;

        Vector3 directionToEnemy = (transform.position - _cameraTransform.position).normalized;
        float angle = Vector3.Angle(_cameraTransform.forward, directionToEnemy);

        if (angle > fovAngle * 0.5f)
            return false;

        if (checkLOS)
        {
            Vector3 direction = transform.position - _cameraTransform.position;
            float distance = direction.magnitude;

            if (Physics.Raycast(_cameraTransform.position, direction.normalized, out RaycastHit hit, distance, obstacles))
            {
                if (!hit.collider.TryGetComponent<EnemigoBase>(out _))
                    return false;
            }
        }

        return true;
    }

    // ── Maquina de estados ─────────────────────────────────────────

    protected void HandleChaseState(bool playerDetected)
    {
        if (playerDetected)
        {
            _isChasing = true;
            _loseSightTimer = 0f;
        }

        if (_isChasing)
        {
            _currentState = EnemyStateMachine.Chasing;
            _agent.speed = _speed;
            _agent.destination = player.transform.position;

            if (!playerDetected)
            {
                _loseSightTimer += Time.deltaTime;
                if (_loseSightTimer >= _loseSightTime)
                    _isChasing = false;
            }
        }

        if (!_isChasing)
            Patrol();
    }

    private void Patrol()
    {
        if (_patrolPoints == null || _patrolPoints.Length == 0)
        {
            _agent.speed = 0f;
            if (_agent.hasPath)
                _agent.ResetPath();
            return;
        }

        Transform target = _patrolPoints[_currentPatrolIndex];
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= _patrolStopDistance)
        {
            _patrolWaitTimer += Time.deltaTime;
            _agent.speed = 0f;
            if (_agent.hasPath)
                _agent.ResetPath();

            if (_patrolWaitTimer >= _patrolWaitTime)
            {
                _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Length;
                _patrolWaitTimer = 0f;
            }
        }
        else
        {
            _currentState = EnemyStateMachine.Idle;
            _agent.speed = _speed;
            _agent.destination = target.position;
        }
    }

    public EnemyState SaveState()
    {
        return new EnemyState
        {
            IsDead = _life <= 0
        };
    }

    public void RestoreState(EnemyState state)
    {
        if (state.IsDead)
        {
            Die();
        }
    }
}
