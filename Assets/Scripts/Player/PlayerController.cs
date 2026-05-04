using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using UnityEngine.XR;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float walkSpeed = 5f;
    public float crouchSpeed = 2.5f;

    [Header("Salto")]
    public float jumpHeight = 1.5f;
    public float gravityMultiplier = 2.5f;

    [Header("Agacharse")]
    public float standingHeight = 2f;
    public float crouchHeight = 1f;

    private CharacterController _controller;
    private Vector3 _verticalVelocity;
    private bool _isCrouching;
    private PlayerInputActions _input;

    private float Gravity => Physics.gravity.y * gravityMultiplier;

    private IEventService _events;


    // ── Ciclo de vida ────────────────────────────────────────────────────────

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _events = AppContainer.Get<IEventService>();
        // Solo ajustamos la altura, el centro lo gestiona Unity según el Inspector
        _controller.height = standingHeight;
        _input = AppContainer.Get<IPlayerInput>().Actions;
    }

    private void OnEnable()
    {
        if (_input == null) return;

        _input.Player.Jump.performed += OnJumpPerformed;
        _input.Player.Crouch.performed += OnCrouchPerformed;
    }

    private void OnDisable()
    {
        if (_input == null) return;

        _input.Player.Jump.performed -= OnJumpPerformed;
        _input.Player.Crouch.performed -= OnCrouchPerformed;
    }

    private void FixedUpdate()
    {
        HandleMovement();
        ApplyGravity();
    }

    // ── Input callbacks ──────────────────────────────────────────────────────

    private void OnJumpPerformed(InputAction.CallbackContext _)
    {
        if (!_controller.isGrounded || _isCrouching) return;
        _verticalVelocity.y = Mathf.Sqrt(2f * Mathf.Abs(Gravity) * jumpHeight);
    }

    private void OnCrouchPerformed(InputAction.CallbackContext _)
    {
        if (_isCrouching)
            TryStand();
        else
            Crouch();
    }

    // ── Agacharse / Levantarse ───────────────────────────────────────────────

    private void Crouch()
    {
        _isCrouching = true;
        _controller.height = crouchHeight;

        _events.Publish(new OnPlayerCrouch());
    }

    private void TryStand()
    {
        if (!CanStandUp()) return;

        _isCrouching = false;
        _controller.height = standingHeight;

        _events.Publish(new OnPlayerStand());
    }

    private bool CanStandUp()
    {
        Vector3 origin = transform.position + Vector3.up * _controller.height;
        float castHeight = standingHeight - crouchHeight;
        return !Physics.SphereCast(origin, _controller.radius * 0.9f, Vector3.up, out _, castHeight);
    }

    // ── Movimiento ───────────────────────────────────────────────────────────

    private void HandleMovement()
    {

        Vector2 input = _input.Player.Move.ReadValue<Vector2>();
        Vector3 direction = transform.right * input.x + transform.forward * input.y;

        if (direction.magnitude > 1f) direction.Normalize();

        float speed = _isCrouching ? crouchSpeed : walkSpeed;
        _controller.Move(direction * speed * Time.deltaTime);
    }

    // ── Gravedad ─────────────────────────────────────────────────────────────

    private void ApplyGravity()
    {
        if (_controller.isGrounded && _verticalVelocity.y < 0f)
            _verticalVelocity.y = -2f;

        _verticalVelocity.y += Gravity * Time.deltaTime;
        _controller.Move(_verticalVelocity * Time.deltaTime);
    }
}