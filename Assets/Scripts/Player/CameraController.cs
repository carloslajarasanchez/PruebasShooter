using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Sensibilidad")]
    public float mouseSensitivity = 0.1f;

    [Header("Límites de rotación vertical")]
    public float lookUpLimit = 80f;
    public float lookDownLimit = 80f;

    [Header("Alturas de cámara")]
    public float standingCameraY = 0.8f;
    public float crouchCameraY = 0.2f;
    public float crouchTransitionSpeed = 8f;

    [Header("Smoothing")]
    [Tooltip("Qué tan suave es el movimiento. Menor = más suave pero más lento")]
    public float smoothSpeed = 8f;
    [Tooltip("Suavizado extra para el eje vertical")]
    public float verticalSmoothSpeed = 6f;

    [Header("Tilt lateral")]
    [Tooltip("Cuánto se inclina la cámara al girar")]
    public float tiltAmount = 3f;
    [Tooltip("Velocidad de vuelta al centro")]
    public float tiltReturnSpeed = 6f;
    [Tooltip("Suavizado de la inclinación")]
    public float tiltSmoothSpeed = 8f;

    [Header("Vibración de pasos")]
    public float stepBobFrequency = 6f;
    public float stepBobAmount = 0.04f;
    public float crouchStepBobFrequency = 4f;
    public float crouchStepBobAmount = 0.02f;
    public float stepBobReturnSpeed = 6f;

    // ── Estado interno ───────────────────────────────────────────────────────

    private float _targetVerticalAngle;
    private float _currentVerticalAngle;

    private float _targetTilt;
    private float _currentTilt;

    private float _targetBodyAngleY;
    private float _currentBodyAngleY;

    private float _targetCameraY;
    private bool _isTransitioning;

    private float _stepBobTimer;
    private float _stepBobCurrentY;
    private bool _isCrouching;

    private Transform _playerBody;
    private PlayerInputActions _input;
    private FootstepController _footstepController;
    private IEventService _events;

    // ── Ciclo de vida ────────────────────────────────────────────────────────

    private void Awake()
    {
        _input = AppContainer.Get<IPlayerInput>().Actions;
        _events = AppContainer.Get<IEventService>();
        _footstepController = GetComponentInParent<FootstepController>();

        _playerBody = transform.parent;
        _targetCameraY = standingCameraY;

        _currentBodyAngleY = _playerBody.eulerAngles.y;
        _targetBodyAngleY = _currentBodyAngleY;

        SetCameraY(standingCameraY);
    }

    private void OnEnable()
    {
        _events.Subscribe<OnPlayerCrouch>(OnCrouch);
        _events.Subscribe<OnPlayerStand>(OnStand);
    }

    private void OnDisable()
    {
        _events.Unsubscribe<OnPlayerCrouch>(OnCrouch);
        _events.Unsubscribe<OnPlayerStand>(OnStand);
    }

    private void FixedUpdate()
    {
        HandleLook();
        HandleStepBob();

        if (_isTransitioning)
            HandleCameraTransition();
    }

    // ── Eventos de agacharse ─────────────────────────────────────────────────

    private void OnCrouch(OwnEventBase parameters)
    {
        _isCrouching = true;
        _targetCameraY = crouchCameraY;
        _isTransitioning = true;
    }

    private void OnStand(OwnEventBase parameters)
    {
        _isCrouching = false;
        _targetCameraY = standingCameraY;
        _isTransitioning = true;
    }

    // ── Transición de altura ─────────────────────────────────────────────────

    private void HandleCameraTransition()
    {
        float newY = Mathf.Lerp(
            transform.localPosition.y, _targetCameraY, crouchTransitionSpeed * Time.deltaTime);

        SetCameraY(newY);

        if (Mathf.Abs(newY - _targetCameraY) < 0.001f)
        {
            SetCameraY(_targetCameraY);
            _isTransitioning = false;
        }
    }

    private void SetCameraY(float y)
    {
        Vector3 pos = transform.localPosition;
        pos.y = y;
        transform.localPosition = pos;
    }

    public void SetVerticalRotation(float angle)
    {
        _targetVerticalAngle = angle;
        _currentVerticalAngle = angle;
        transform.localEulerAngles = new Vector3(_currentVerticalAngle, 0f, 0f);
    }

    // ── Look ─────────────────────────────────────────────────────────────────

    private void HandleLook()
    {
        Vector2 delta = _input.Player.Camera.ReadValue<Vector2>() * mouseSensitivity;

        // Rotación horizontal con smoothing
        _targetBodyAngleY += delta.x;
        _currentBodyAngleY = Mathf.LerpAngle(
            _currentBodyAngleY, _targetBodyAngleY, smoothSpeed * Time.deltaTime);
        _playerBody.eulerAngles = new Vector3(0f, _currentBodyAngleY, 0f);

        // Rotación vertical con smoothing
        _targetVerticalAngle -= delta.y;
        _targetVerticalAngle = Mathf.Clamp(_targetVerticalAngle, -lookUpLimit, lookDownLimit);
        _currentVerticalAngle = Mathf.Lerp(
            _currentVerticalAngle, _targetVerticalAngle, verticalSmoothSpeed * Time.deltaTime);

        // Tilt lateral — se inclina en dirección contraria al giro para dar sensación de peso
        _targetTilt = -delta.x * tiltAmount;
        _currentTilt = Mathf.Lerp(_currentTilt, _targetTilt, tiltSmoothSpeed * Time.deltaTime);

        if (Mathf.Abs(delta.x) < 0.01f)
            _currentTilt = Mathf.Lerp(_currentTilt, 0f, tiltReturnSpeed * Time.deltaTime);

        transform.localEulerAngles = new Vector3(_currentVerticalAngle, 0f, _currentTilt);
    }

    // ── Step Bob ─────────────────────────────────────────────────────────────

    private void HandleStepBob()
    {
        Vector2 moveInput = _input.Player.Move.ReadValue<Vector2>();
        bool isMoving = moveInput.magnitude > 0.1f;

        if (isMoving)
        {
            float frequency = _isCrouching ? crouchStepBobFrequency : stepBobFrequency;
            float amount = _isCrouching ? crouchStepBobAmount : stepBobAmount;

            _stepBobTimer += Time.deltaTime * frequency;
            _stepBobCurrentY = Mathf.Sin(_stepBobTimer) * amount;
        }
        else
        {
            // Vuelve suavemente a 0 al parar
            _stepBobTimer = 0f;
            _stepBobCurrentY = Mathf.Lerp(_stepBobCurrentY, 0f, Time.deltaTime * stepBobReturnSpeed);
        }

        // Notificamos al FootstepController con el valor actual del seno
        _footstepController?.OnBobUpdate(Mathf.Sin(_stepBobTimer), isMoving);

        // Sumamos el bob al Y objetivo sin interferir con la transición de crouch
        Vector3 pos = transform.localPosition;
        pos.y = _targetCameraY + _stepBobCurrentY;
        transform.localPosition = pos;
    }
}