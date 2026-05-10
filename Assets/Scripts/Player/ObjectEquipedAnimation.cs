using UnityEngine;

public class ObjectEquipedAnimation : MonoBehaviour
{
    [Header("Sway - Ratón")]
    public float swayAmount = 0.02f;
    public float swaySpeed = 8f;
    public float swayClamp = 0.1f;

    [Header("Sway - Movimiento")]
    public float movementSwayAmount = 0.03f;

    [Header("Bob - Caminar")]
    public float bobFrequency = 8f;
    public float bobHorizontalAmount = 0.05f;
    public float bobVerticalAmount = 0.03f;

    [Header("Bob - Agachado")]
    public float crouchBobFrequency = 5f;
    public float crouchBobAmount = 0.02f;

    [Header("Retroceso al disparar")]
    public float recoilAmount = 0.05f;
    public float recoilSpeed = 10f;
    public float recoilReturnSpeed = 6f;

    private Vector3 _initialPosition;
    private Quaternion _initialRotation;

    private Vector3 _swayOffset;
    private Vector3 _bobOffset;
    private Vector3 _recoilOffset;

    private float _bobTimer;
    private bool _isCrouching;

    private PlayerInputActions _input;
    private IEventService _eventService;

    private void Awake()
    {
        _input = AppContainer.Get<IPlayerInput>().Actions;
        _eventService = AppContainer.Get<IEventService>();

        _initialPosition = transform.localPosition;
        _initialRotation = transform.localRotation;
    }

    private void OnEnable()
    {
        _eventService.Subscribe<OnPlayerCrouch>(OnCrouch);
        _eventService.Subscribe<OnPlayerStand>(OnStand);
        _eventService.Subscribe<OnPlayerShoot>(OnShoot); // si tienes este evento
    }

    private void OnDisable()
    {
        _eventService.Unsubscribe<OnPlayerCrouch>(OnCrouch);
        _eventService.Unsubscribe<OnPlayerStand>(OnStand);
        _eventService.Unsubscribe<OnPlayerShoot>(OnShoot);
    }

    private void Update()
    {
        HandleSway();
        HandleBob();
        HandleRecoil();
        ApplyOffsets();
    }

    // ── Sway ─────────────────────────────────────────────────────────────────

    private void HandleSway()
    {
        Vector2 mouseDelta = _input.Player.Camera.ReadValue<Vector2>();
        Vector2 moveInput = _input.Player.Move.ReadValue<Vector2>();

        // Sway por ratón
        float swayX = Mathf.Clamp(-mouseDelta.x * swayAmount, -swayClamp, swayClamp);
        float swayY = Mathf.Clamp(-mouseDelta.y * swayAmount, -swayClamp, swayClamp);

        // Sway adicional por movimiento
        float moveSway = -moveInput.x * movementSwayAmount;

        _swayOffset = new Vector3(swayX + moveSway, swayY, 0f);
    }

    // ── Bob ──────────────────────────────────────────────────────────────────

    private void HandleBob()
    {
        Vector2 moveInput = _input.Player.Move.ReadValue<Vector2>();
        bool isMoving = moveInput.magnitude > 0.1f;

        if (isMoving)
        {
            float frequency = _isCrouching ? crouchBobFrequency : bobFrequency;
            float amount = _isCrouching ? crouchBobAmount : bobVerticalAmount;

            _bobTimer += Time.deltaTime * frequency;

            float bobX = Mathf.Cos(_bobTimer) * bobHorizontalAmount;
            float bobY = Mathf.Abs(Mathf.Sin(_bobTimer)) * amount; // Abs para que solo suba y baje

            _bobOffset = new Vector3(bobX, -bobY, 0f);
        }
        else
        {
            // Vuelve suavemente a cero cuando para
            _bobTimer = 0f;
            _bobOffset = Vector3.Lerp(_bobOffset, Vector3.zero, Time.deltaTime * swaySpeed);
        }
    }

    // ── Recoil ───────────────────────────────────────────────────────────────

    private void HandleRecoil()
    {
        _recoilOffset = Vector3.Lerp(_recoilOffset, Vector3.zero, Time.deltaTime * recoilReturnSpeed);
    }

    // ── Aplicar todo ─────────────────────────────────────────────────────────

    private void ApplyOffsets()
    {
        Vector3 targetPosition = _initialPosition + _swayOffset + _bobOffset + _recoilOffset;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * swaySpeed);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, _initialRotation, Time.deltaTime * swaySpeed);
    }

    // ── Eventos ───────────────────────────────────────────────────────────────

    private void OnCrouch(OwnEventBase parameters) => _isCrouching = true;
    private void OnStand(OwnEventBase parameters) => _isCrouching = false;

    private void OnShoot(OwnEventBase parameters)
    {
        _recoilOffset = new Vector3(0f, 0f, -recoilAmount);
    }

    // Método público por si quieres disparar el recoil desde otro script
    public void ApplyRecoil()
    {
        _recoilOffset = new Vector3(0f, 0f, -recoilAmount);
    }
}
