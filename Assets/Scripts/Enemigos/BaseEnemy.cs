using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class BaseEnemy : MonoBehaviour, ISavable<EnemyState>, IPusheable
{
    // ── Configuración general ─────────────────────────────────────
    [SerializeField] private float _life;
    [SerializeField] private int _damage;
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
    private IPlayer _player;

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
    [SerializeField] private float _investigationLookTime = 3f;
    private float _lookAroundTimer = 0f;
    private bool _searchingStarted = false;

    [Header("Ataque")]
    [SerializeField] private float _attackRange = 1.5f;
    [SerializeField] private float _attackCooldown = 2f;
    private float _attackTimer = 0f;
    private bool _hasDealtDamage;

    public bool CanDealDamage { get; protected set; }
    public int GetDamage() => _damage;

    [Header("Desmembramiento")]
    [SerializeField] private DismembermentMode _dismembermentMode = DismembermentMode.None;
    [SerializeField] private LimbData[] _limbConfigs;
    private Dictionary<HumanBodyBones, LimbData> _limbMap;
    protected bool _isDead;

    // ── Propiedades ───────────────────────────────────────────────
    public string SaveId => _saveId;
    protected EnemyStateMachine CurrentState => _currentState;

    private Transform DetectionOrigin => _headTransform != null ? _headTransform : transform;

    // ── Ciclo de vida Unity ───────────────────────────────────────
    private void Awake()
    {
        if (string.IsNullOrEmpty(_saveId))
            _saveId = System.Guid.NewGuid().ToString();

        _saveService = AppContainer.Get<ISaveService>();
        _animator = GetComponent<Animator>();
        _eventService = AppContainer.Get<IEventService>();
        _player = AppContainer.Get<IPlayer>();
        _hitRig = GetComponent<HitReactionRig>();
        if (_hitRig == null)
            _hitRig = gameObject.AddComponent<HitReactionRig>();

        InitializeLimbMap();
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

        // Alertar al enemigo solo si no estaba ya en combate activo.
        // No interrumpimos Attacking para no cancelar animaciones a mitad.
        if (_currentState != EnemyStateMachine.Chasing &&
            _currentState != EnemyStateMachine.Attacking)
        {
            TransitionToChase();
        }

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
    protected bool IsPlayerInVisionCone(float range, float angle, float height, bool checkLOS, LayerMask obstacles)
    {
        if (player == null) return false;

        Vector3 origin = DetectionOrigin.position;
        Vector3 headForward = DetectionOrigin.forward;

        // Apuntamos al centro de masa del jugador (1m sobre sus pies), no a los pies.
        // Así el raycast no choca con el suelo ni con geometría baja cerca del jugador.
        Vector3 playerCenter = player.transform.position + Vector3.up * 1f;
        Vector3 dirToPlayer = (playerCenter - origin).normalized;
        float distance = Vector3.Distance(origin, playerCenter);

        if (distance > range) return false;

        // Con ángulo 360 no hace falta chequear dirección (usado en combate)
        if (angle < 360f && Vector3.Angle(headForward, dirToPlayer) > angle * 0.5f) return false;

        if (checkLOS)
        {
            // El origen del rayo ya es la cabeza; height solo se usa si la subclase
            // lo necesita por compatibilidad (valor 0 = sin offset extra).
            Vector3 rayOrigin = origin + Vector3.up * height;
            if (Physics.Raycast(rayOrigin, dirToPlayer, out RaycastHit hit, distance, obstacles))
            {
                if (!hit.collider.TryGetComponent<BaseEnemy>(out _) &&
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
                if (!hit.collider.TryGetComponent<BaseEnemy>(out _))
                    return false;
            }
        }

        return true;
    }

    // ── Máquina de estados ────────────────────────────────────────
    protected void HandleChaseState(bool playerDetected)
    {
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
                if (playerDetected)
                    TransitionToChase();
                else
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

            if (_attackTimer > 0f)
                _attackTimer -= Time.deltaTime;

            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer <= _attackRange && _attackTimer <= 0f)
            {
                TransitionToAttack();
                return;
            }

            _agent.speed = _speed * _chaseSpeedMultiplier;
            _agent.destination = player.transform.position;
            _headAim?.SetTracking(player.transform);
            return;
        }

        _loseSightTimer += Time.deltaTime;
        _lastKnownPlayerPosition = player.transform.position;
        _agent.speed = _speed * _chaseSpeedMultiplier;
        _agent.destination = _lastKnownPlayerPosition;

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
        _hasDealtDamage = false;

        if (player != null)
            _headAim?.SetTracking(player.transform);
    }

    private void TransitionToInvestigation()
    {
        _isChasing = false;
        _currentState = EnemyStateMachine.Investigating;
        _investigationTimer = 0f;
        _lookAroundTimer = _investigationLookTime;
        _searchingStarted = false;
        _animator.SetBool("isChasing", false);
        _agent.SetDestination(_lastKnownPlayerPosition);

        _headAim?.SetSearching(_lastKnownPlayerPosition);
        _searchingStarted = true;
    }

    private void TransitionToChase()
    {
        _isChasing = true;
        _loseSightTimer = 0f;
        _investigationTimer = 0f;
        _lookAroundTimer = 0f;
        _searchingStarted = false;
        _currentState = EnemyStateMachine.Chasing;
        _animator.SetBool("isChasing", true);

        if (player != null)
            _headAim?.SetTracking(player.transform);
    }

    private void HandleAttackState(bool playerDetected)
    {
        _agent.speed = 0f;
        if (_agent.hasPath) _agent.ResetPath();

        if (playerDetected && player != null)
            _headAim?.SetTracking(player.transform);

        // Salida gestionada por OnAttackFinished (Animation Event)
    }

    // ── Animation Events ──────────────────────────────────────────
    public void EnableDamage() => CanDealDamage = true;
    public void DisableDamage() => CanDealDamage = false;

    public void OnAttackFinished()
    {
        CanDealDamage = false;
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

        // SetSearching ya se llamó en TransitionToInvestigation, no repetir
        _lookAroundTimer -= Time.deltaTime;
        if (_lookAroundTimer <= 0f)
        {
            _searchingStarted = false;
            _currentState = EnemyStateMachine.Idle;
            _headAim?.SetIdle();
        }
    }

    private void Patrol()
    {
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

    public virtual void OnHitReaction(HumanBodyBones bone, Vector3 force, Rigidbody boneRb, float damage)
    {
        if (_isDead)
        {
            Push(force*0.5f);
            return;
        }

        _hitRig?.TriggerHit(bone, force*Time.deltaTime);
        if (_dismembermentMode != DismembermentMode.None)
            EvaluateDismemberment(bone, damage, force);
    }

    private void InitializeLimbMap()
    {
        _limbMap = new Dictionary<HumanBodyBones, LimbData>();
        if (_hitRig == null) return;

        var groupRoots = _hitRig.GetBoneGroupRoots();
        foreach (var root in groupRoots)
        {
            var limb = new LimbData { bone = root };
            limb.Initialize();

            if (root == HumanBodyBones.Head ||
                root == HumanBodyBones.UpperChest ||
                root == HumanBodyBones.Spine)
                limb.isCentral = true;

            _limbMap[root] = limb;
        }

        if (_limbConfigs == null || _limbConfigs.Length == 0) return;

        foreach (var config in _limbConfigs)
        {
            if (_limbMap.TryGetValue(config.bone, out var existing))
            {
                existing.maxHealth = config.maxHealth;
                existing.instantSeverForce = config.instantSeverForce;
                existing.isCentral = config.isCentral;
                existing.Initialize();
            }
            else
            {
                config.Initialize();
                _limbMap[config.bone] = config;
            }
        }
    }

    private void EvaluateDismemberment(HumanBodyBones bone, float damage, Vector3 force)
    {
        HumanBodyBones groupRoot = ResolveGroupRoot(bone);
        if (!_limbMap.TryGetValue(groupRoot, out var limb)) return;
        if (limb.isSevered) return;

        limb.currentHealth -= damage;

        bool severedByForce = force.magnitude >= limb.instantSeverForce;
        bool severedByHealth = limb.currentHealth <= 0f;

        if (severedByForce || severedByHealth)
            Dismember(limb, groupRoot, force);
    }

    private void Dismember(LimbData limb, HumanBodyBones groupRoot, Vector3 force)
    {
        limb.isSevered = true;

        switch (_dismembermentMode)
        {
            case DismembermentMode.Dangle:
                _hitRig?.DangleLimb(groupRoot);
                break;
            default:
                _hitRig?.SeverLimb(groupRoot, force);
                break;
        }

        if (limb.isCentral)
            Die();
    }

    private static HumanBodyBones ResolveGroupRoot(HumanBodyBones bone)
    {
        return bone switch
        {
            HumanBodyBones.Neck => HumanBodyBones.Head,
            HumanBodyBones.Chest => HumanBodyBones.UpperChest,
            HumanBodyBones.Spine => HumanBodyBones.UpperChest,
            HumanBodyBones.LeftShoulder => HumanBodyBones.LeftUpperArm,
            HumanBodyBones.LeftLowerArm => HumanBodyBones.LeftUpperArm,
            HumanBodyBones.LeftHand => HumanBodyBones.LeftUpperArm,
            HumanBodyBones.RightShoulder => HumanBodyBones.RightUpperArm,
            HumanBodyBones.RightLowerArm => HumanBodyBones.RightUpperArm,
            HumanBodyBones.RightHand => HumanBodyBones.RightUpperArm,
            HumanBodyBones.LeftLowerLeg => HumanBodyBones.LeftUpperLeg,
            HumanBodyBones.LeftFoot => HumanBodyBones.LeftUpperLeg,
            HumanBodyBones.LeftToes => HumanBodyBones.LeftUpperLeg,
            HumanBodyBones.RightLowerLeg => HumanBodyBones.RightUpperLeg,
            HumanBodyBones.RightFoot => HumanBodyBones.RightUpperLeg,
            HumanBodyBones.RightToes => HumanBodyBones.RightUpperLeg,
            _ => bone
        };
    }

    protected virtual void HandlePlayerCrouch(OwnEventBase eventBase) { }

    // ── Guardado ──────────────────────────────────────────────────
    public EnemyState SaveState() => new EnemyState { IsDead = _life <= 0 };

    public void RestoreState(EnemyState state)
    {
        if (state.IsDead) Die();
    }
}