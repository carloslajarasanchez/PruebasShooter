using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class EnemigoBase : MonoBehaviour, ISavable<EnemyState>, IPusheable
{
    // ── Configuración general ─────────────────────────────────────
    [SerializeField] private float _life;
    [SerializeField] private float _damage;
    [SerializeField] protected float _speed;
    [SerializeField] private string _saveId;
    [SerializeField] protected Animator _animator;

    // ── Referencias ───────────────────────────────────────────────
    protected NavMeshAgent _agent;
    protected Transform _cameraTransform;
    protected EnemyStateMachine _currentState;
    public GameObject player;
    private ISaveService _saveService;
    private IEventService _eventService;

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
    [SerializeField] private float _chaseSpeedMultiplier = 1.2f;

    private float _loseSightTimer;
    protected bool _isChasing;
    protected Vector3 _lastKnownPlayerPosition;

    // ── Investigación ─────────────────────────────────────────────
    [Header("Investigacion")]
    [SerializeField] private float _investigationSpeed;
    [SerializeField] private float _investigationLookSpeed = 120f;
    [SerializeField] private float _investigationDistanceThreshold = 1.5f;
    [SerializeField] private float _investigationPauseTime = 0.5f;
    private float _investigationTimer = 0f;
    [SerializeField] private float _maxInvestigationTime = 5f;

    [Header("Ataque")]
    [SerializeField] private float _attackRange = 1.5f;
    [SerializeField] private float _attackCooldown = 2f;
    private float _attackTimer = 0f;

    private Quaternion _startRotation;
    private int _investigationLookIndex;
    private float _lookTimer;
    protected bool _isDead;

    // ── Propiedades ───────────────────────────────────────────────
    public string SaveId => _saveId;

    // ── Ciclo de vida Unity ───────────────────────────────────────
    private void Awake()
    {
        if (string.IsNullOrEmpty(_saveId))
            _saveId = System.Guid.NewGuid().ToString();

        _saveService = AppContainer.Get<ISaveService>();
        _animator = GetComponent<Animator>();
        _eventService = AppContainer.Get<IEventService>();
    }

    protected virtual void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _eventService?.Subscribe<OnPlayerCrouch>(HandlePlayerCrouch);
        _agent.speed = _speed;

        if (_investigationSpeed <= 0f)
            _investigationSpeed = _speed * 0.8f;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (Camera.main != null)
            _cameraTransform = Camera.main.transform;
    }

    protected virtual void Update() { }



    // ── Vida y muerte ─────────────────────────────────────────────
    public virtual void TakeDamage(float damage)
    {
        if (_isDead) return;

        Debug.Log($"{name} recibió {damage} de daño.");

        _life -= damage;

        HandleChaseState(true);

        if (_life <= 0)
            Die();
    }

    private void Die()
    {
        if (_isDead) return;

        _isDead = true;

        _saveService?.SetState(SaveId, SaveState());
        _animator.enabled = false;
        _agent.enabled = false;
        EnableRagdoll();
    }


    private void EnableRagdoll()
    {
        // Activa física en todos los rigidbodies de los huesos
        foreach (var rb in GetComponentsInChildren<Rigidbody>())
            rb.isKinematic = false;

        // Activa todos los colliders de los huesos
        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = true;
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
        return Vector3.Distance(transform.position, player.transform.position) <= range;
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
        if (playerDetected && _currentState != EnemyStateMachine.Chasing)
        {
            TransitionToChase();
        }
        switch (_currentState)
        {
            case EnemyStateMachine.Chasing:
                UpdateChaseState(playerDetected);
                break;
            case EnemyStateMachine.Investigating:
                HandleInvestigationState(playerDetected);
                break;
            case EnemyStateMachine.Attacking:
                HandleAttackState(playerDetected);
                break;
            default:
                Patrol();
                break;
        }
    }

    private void UpdateChaseState(bool playerDetected)
    {
        if (playerDetected)
        {
            _loseSightTimer = 0f;
            _lastKnownPlayerPosition = player.transform.position;

            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            if (distanceToPlayer <= _attackRange && _attackTimer <= 0f)
            {
                TransitionToAttack();
                return;
            }

            _attackTimer -= Time.deltaTime;
            _agent.speed = _speed * _chaseSpeedMultiplier;
            _agent.destination = player.transform.position;
            return;
        }

        _loseSightTimer += Time.deltaTime;
        if (_loseSightTimer >= _loseSightTime)
            TransitionToInvestigation();
    }

    private void TransitionToAttack()
    {
        _currentState = EnemyStateMachine.Attacking;
        _agent.speed = 0f;
        if (_agent.hasPath) _agent.ResetPath();
        _animator.SetTrigger("attack");
        _attackTimer = _attackCooldown;
    }
    private void TransitionToInvestigation()
    {
        _isChasing = false;
        _currentState = EnemyStateMachine.Investigating;
        _startRotation = transform.rotation;
        _investigationLookIndex = 0;
        _lookTimer = 0f;
        _investigationTimer = 0f;
        _animator.SetBool("isChasing", false);
        _agent.SetDestination(_lastKnownPlayerPosition);
    }

    private void TransitionToChase()
    {
        _isChasing = true;
        _loseSightTimer = 0f;
        _investigationTimer = 0f;
        _currentState = EnemyStateMachine.Chasing;
        _animator.SetBool("isChasing", true);
    }

    private void HandleAttackState(bool playerDetected)
    {
        _agent.speed = 0f;
        if (_agent.hasPath) _agent.ResetPath();
        _attackTimer -= Time.deltaTime;

        // Cuando termina el cooldown vuelve a perseguir
        if (_attackTimer <= 0f)
            TransitionToChase();
    }
    private void HandleInvestigationState(bool playerDetected)
    {
        if (playerDetected)
        {
            TransitionToChase();
            return;
        }

        float distanceToLastPos = Vector3.Distance(transform.position, _lastKnownPlayerPosition);
        bool timedOut = _investigationTimer >= _maxInvestigationTime;
        bool arrivedAtPoint = distanceToLastPos <= _investigationDistanceThreshold;

        if (!arrivedAtPoint && !timedOut)
        {
            _investigationTimer += Time.deltaTime;
            _agent.speed = _investigationSpeed;
            _agent.destination = _lastKnownPlayerPosition;
            return;
        }

        _agent.speed = 0f;
        if (_agent.hasPath) _agent.ResetPath();
        LookAround();
    }

    private void LookAround()
    {
        float[] angles = { 90f, -90f, 180f };

        if (_investigationLookIndex >= angles.Length)
        {
            _currentState = EnemyStateMachine.Idle;
            return;
        }

        Quaternion targetRot = _startRotation * Quaternion.Euler(0f, angles[_investigationLookIndex], 0f);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, targetRot, _investigationLookSpeed * Time.deltaTime);

        if (Quaternion.Angle(transform.rotation, targetRot) < 2f)
        {
            _lookTimer += Time.deltaTime;
            if (_lookTimer >= _investigationPauseTime)
            {
                _investigationLookIndex++;
                _lookTimer = 0f;
            }
        }
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

    public void OnDisable()
    {
        _eventService?.Unsubscribe<OnPlayerCrouch>(HandlePlayerCrouch);
    }
    public void Push(Vector3 force)
    {
        if (!_isDead) return;
        foreach (var rb in GetComponentsInChildren<Rigidbody>())
        {
            if (!rb.isKinematic)
                rb.AddForce(force, ForceMode.Impulse);
                rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, 10f);
        }
    }

    protected virtual void HandlePlayerCrouch(OwnEventBase eventBase) { }

    // ── Guardado ──────────────────────────────────────────────────
    public EnemyState SaveState() => new EnemyState { IsDead = _life <= 0 };

    public void RestoreState(EnemyState state)
    {
        if (state.IsDead) Die();
    }
}