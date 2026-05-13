using System.Collections;
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

    // ── Cabeza (aim constraint) ───────────────────────────────────
    [Header("Cabeza y Aim Constraint")]
    [SerializeField] private Transform _headTransform;
    [SerializeField] private EnemyHeadAim _headAim;

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

    protected bool _isDead;

    // ── Propiedades ───────────────────────────────────────────────
    public string SaveId => _saveId;

    private Transform DetectionOrigin => _headTransform != null ? _headTransform : transform;

    // ── Ciclo de vida Unity ───────────────────────────────────────
    private void Awake()
    {
        if (string.IsNullOrEmpty(_saveId))
            _saveId = System.Guid.NewGuid().ToString();

        _saveService = AppContainer.Get<ISaveService>();
        _animator = GetComponent<Animator>();
        _eventService = AppContainer.Get<IEventService>();
        _hitRig = GetComponent<HitReactionRig>();
        if (_hitRig == null)
            _hitRig = gameObject.AddComponent<HitReactionRig>();
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

        if (_headAim == null)
            _headAim = GetComponentInChildren<EnemyHeadAim>();
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
        StopAllCoroutines();
        _saveService?.SetState(SaveId, SaveState());
        _animator.enabled = false;
        if (_hitRig != null) _hitRig.enabled = false;
        _agent.enabled = false;

        // Apaga el aim de cabeza
        _headAim?.SetIdle();

        EnableRagdoll();
    }

    private void EnableRagdoll()
    {
        foreach (var rb in GetComponentsInChildren<Rigidbody>())
            rb.isKinematic = false;

        foreach (var col in GetComponentsInChildren<Collider>())
        {
            col.enabled = true;
            col.isTrigger = false;
        }
    }

    // ── Detección ─────────────────────────────────────────────────
    /// <summary>
    /// Cono de visión usando el forward y la posición del hueso de la cabeza.
    /// </summary>
    protected bool IsPlayerInVisionCone(float range, float angle, float height, bool checkLOS, LayerMask obstacles)
    {
        if (player == null) return false;

        Vector3 origin = DetectionOrigin.position;
        // Dirección forward de la cabeza para el ángulo de visión
        Vector3 headForward = DetectionOrigin.forward;

        Vector3 dirToPlayer = (player.transform.position - origin).normalized;
        float distance = Vector3.Distance(origin, player.transform.position);

        if (distance > range) return false;
        if (Vector3.Angle(headForward, dirToPlayer) > angle * 0.5f) return false;

        if (checkLOS)
        {
            // El offset de altura ya no es necesario si usamos la cabeza directamente,
            // pero se respeta por compatibilidad con subclases que pasen height > 0.
            Vector3 rayOrigin = origin + Vector3.up * height;
            if (Physics.Raycast(rayOrigin, dirToPlayer, out RaycastHit hit, distance, obstacles))
            {
                if (!hit.collider.TryGetComponent<EnemigoBase>(out _) &&
                    !hit.collider.CompareTag("Player"))
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
            if (Physics.Raycast(_cameraTransform.position, direction.normalized,
                                out RaycastHit hit, direction.magnitude, obstacles))
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

            // Cabeza sigue al jugador mientras persigue
            _headAim?.SetTracking(player.transform);
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

        // Durante el ataque la cabeza sigue mirando al jugador
        if (player != null)
            _headAim?.SetTracking(player.transform);
    }

    private void TransitionToInvestigation()
    {
        _isChasing = false;
        _currentState = EnemyStateMachine.Investigating;
        _investigationTimer = 0f;
        _animator.SetBool("isChasing", false);
        _agent.SetDestination(_lastKnownPlayerPosition);

        // Cabeza empieza a buscar desde la última posición conocida
        _headAim?.SetSearching(_lastKnownPlayerPosition);
    }

    private void TransitionToChase()
    {
        _isChasing = true;
        _loseSightTimer = 0f;
        _investigationTimer = 0f;
        _currentState = EnemyStateMachine.Chasing;
        _animator.SetBool("isChasing", true);

        // Cabeza apunta al jugador inmediatamente
        if (player != null)
            _headAim?.SetTracking(player.transform);
    }

    private void HandleAttackState(bool playerDetected)
    {
        _agent.speed = 0f;
        if (_agent.hasPath) _agent.ResetPath();
        _attackTimer -= Time.deltaTime;

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
        LookAround();
    }

    private void LookAround()
    {
        
        _headAim?.SetSearching(_lastKnownPlayerPosition);

    }

    private void Patrol()
    {
        // En patrulla la cabeza mira al frente (idle)
        _headAim?.SetIdle();

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

    private HitReactionRig _hitRig;

    public void OnHitReaction(HumanBodyBones bone, Vector3 force, Rigidbody boneRb)
    {
        if (_isDead) return;
        _hitRig?.TriggerHit(bone, force);
    }

    protected virtual void HandlePlayerCrouch(OwnEventBase eventBase) { }

    // ── Guardado ──────────────────────────────────────────────────
    public EnemyState SaveState() => new EnemyState { IsDead = _life <= 0 };

    public void RestoreState(EnemyState state)
    {
        if (state.IsDead) Die();
    }
}