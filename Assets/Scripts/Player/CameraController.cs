using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class CameraController : MonoBehaviour
{
    [Header("Sensibility")]
    [Tooltip("Sensibility of the mouse / right stick")]
    public float mouseSensitivity = 0.1f;

    [Header("Vertical rotation limits")]
    [Tooltip("Maximum upward angle")]
    public float lookUpLimit = 80f;
    [Tooltip("Maximum downward angle")]
    public float lookDownLimit = 80f;

    [Header("Alturas de cámara")]
    public float standingCameraY = 0.8f;
    public float crouchCameraY = 0.2f;
    public float crouchTransitionSpeed = 8f;

    private float _verticalAngle;
    private Transform _playerBody;

    // Estado de transición
    private float _targetCameraY;
    private bool _isTransitioning;
    private PlayerInputActions _input;
    private IEventService _events;

    // ── Ciclo de vida ────────────────────────────────────────────────────────

    private void Awake()
    {

        _input = AppContainer.Get<IPlayerInput>().Actions;
        _events = AppContainer.Get<IEventService>();

        _playerBody = transform.parent;
        _targetCameraY = standingCameraY;

        // Posición inicial garantizada sin Lerp
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

    private void Update()
    {
        HandleLook();

        if (_isTransitioning)
            HandleCameraTransition();
    }

    // ── Eventos de agacharse ─────────────────────────────────────────────────

    private void OnCrouch(OwnEventBase parameters)
    {
        _targetCameraY = crouchCameraY;
        _isTransitioning = true;
    }

    private void OnStand(OwnEventBase parameters)
    {
        _targetCameraY = standingCameraY;
        _isTransitioning = true;
    }

    // ── Transición de altura ─────────────────────────────────────────────────

    private void HandleCameraTransition()
    {
        float newY = Mathf.Lerp(
            transform.localPosition.y, _targetCameraY, crouchTransitionSpeed * Time.deltaTime);

        SetCameraY(newY);

        // Detener la transición cuando ya está suficientemente cerca
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

    // ── Look ─────────────────────────────────────────────────────────────────

    private void HandleLook()
    {

        Vector2 delta = _input.Player.Camera.ReadValue<Vector2>() * mouseSensitivity;

        _playerBody.Rotate(Vector3.up * delta.x);

        _verticalAngle -= delta.y;
        _verticalAngle = Mathf.Clamp(_verticalAngle, -lookUpLimit, lookDownLimit);
        transform.localEulerAngles = new Vector3(_verticalAngle, 0f, 0f);
    }
}