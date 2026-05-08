using UnityEngine;
using UnityEngine.AI;

public class EnemigoBase : MonoBehaviour, ISavable<EnemyState>
{
    // ── Configuración general ─────────────────────────────────────
    [SerializeField] private float _life;
    [SerializeField] private float _damage;
    [SerializeField] protected float _speed;
    [SerializeField] private string _saveId;

    // ── Referencias ───────────────────────────────────────────────
    protected NavMeshAgent _agent;
    protected Transform _cameraTransform;
    protected EnemyStateMachine _currentState;
    public GameObject player;
    private ISaveService _saveService;

    // ── Patrullaje ────────────────────────────────────────────────
    [Header("Patrullaje")]
    [SerializeField] private Transform[] _patrolPoints;
    [SerializeField] private float _patrolWaitTime = 2f;
    [SerializeField] private float _patrolStopDistance = 1f;

    private int _currentPatrolIndex;
    private float _patrolWaitTimer;

    // ── Persecución ───────────────────────────────────────────────
    [Header("Persecucion")]
    [SerializeField] private float _loseSightTime = 2f;

    private float _loseSightTimer;
    protected bool _isChasing;

    // ── Propiedades ───────────────────────────────────────────────
    public string SaveId => _saveId;

    // ── Ciclo de vida Unity ───────────────────────────────────────
    private void Awake()
    {
        if (string.IsNullOrEmpty(_saveId))
            _saveId = System.Guid.NewGuid().ToString();

        _saveService = AppContainer.Get<ISaveService>();
    }

    protected virtual void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = _speed;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (Camera.main != null)
            _cameraTransform = Camera.main.transform;
    }

    protected virtual void Update() { }

    // ── Vida y muerte ─────────────────────────────────────────────
    public virtual void TakeDamage(float damage)
    {
        _life -= damage;
        if (_life <= 0)
            Die();
    }

    private void Die()
    {
        _saveService?.SetState(SaveId, SaveState());
        Destroy(gameObject);
    }

    // ── Detección ─────────────────────────────────────────────────
    protected bool IsPlayerInVisionCone(float range, float angle, float height, bool checkLOS, LayerMask obstacles)
    {
        if (player == null) return false;

        Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance > range) return false;
        if (Vector3.Angle(transform.forward, dirToPlayer) > angle * 0.5f) return false;

        if (checkLOS)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * height;
            if (Physics.Raycast(rayOrigin, dirToPlayer, out RaycastHit hit, distance, obstacles))
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

        Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.transform.position);
        float dot = Vector3.Dot(transform.forward, dirToPlayer);

        return dot < 0f && distance <= range;
    }

    protected bool IsPlayerLookingAtMe(float fovAngle, bool checkLOS, LayerMask obstacles)
    {
        if (_cameraTransform == null || player == null) return false;

        Vector3 dirToEnemy = (transform.position - _cameraTransform.position).normalized;
        if (Vector3.Angle(_cameraTransform.forward, dirToEnemy) > fovAngle * 0.5f) return false;

        if (checkLOS)
        {
            Vector3 direction = transform.position - _cameraTransform.position;
            if (Physics.Raycast(_cameraTransform.position, direction.normalized, out RaycastHit hit, direction.magnitude, obstacles))
            {
                if (!hit.collider.TryGetComponent<EnemigoBase>(out _))
                    return false;
            }
        }

        return true;
    }

    // ── Máquina de estados ────────────────────────────────────────
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
            if (_agent.hasPath) _agent.ResetPath();
            return;
        }

        Transform target = _patrolPoints[_currentPatrolIndex];
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= _patrolStopDistance)
        {
            _agent.speed = 0f;
            if (_agent.hasPath) _agent.ResetPath();

            _patrolWaitTimer += Time.deltaTime;
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

    // ── Guardado ──────────────────────────────────────────────────
    public EnemyState SaveState() => new EnemyState { IsDead = _life <= 0 };

    public void RestoreState(EnemyState state)
    {
        if (state.IsDead) Die();
    }
}