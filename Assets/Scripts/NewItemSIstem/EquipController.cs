using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controlador unificado de equipamiento. Gestiona:
///   - Click izquierdo: usar el item equipado (disparo, poción, etc.)
///   - R: recargar si el item es un arma
///   - Rueda del ratón: swap con el item anteriormente equipado
///
/// ACCIONES en el mapa "Player" de PlayerInputActions:
///   - Use          → Button → Left Mouse Button
///   - Reload       → Button → R
///   - SwapItem     → Value, Vector2 → Mouse/Scroll
///
/// Pon este script en el GameObject Player.
/// </summary>
public class EquipController : MonoBehaviour
{
    [SerializeField] private Transform _hand;

    private IPlayerInput _playerInput;
    private IEquipService _equipService;
    private PlayerInputActions _actions;
    private bool _isPressing;

    private void Awake()
    {
        _playerInput = AppContainer.Get<IPlayerInput>();
        _equipService = AppContainer.Get<IEquipService>();

        if (_hand != null)
            _equipService.SetHandTransform(_hand);
        else
            Debug.LogWarning("[EquipController] Hand no asignado en el Inspector.");
    }

    private void OnEnable()
    {
        _actions = _playerInput.Actions;
        _actions.Player.Use.started += OnUseStarted;
        _actions.Player.Use.canceled += OnUseCanceled;
        _actions.Player.Reload.performed += OnReload;
        _actions.Player.SwapItem.performed += OnSwapItem;
    }

    private void OnDisable()
    {
        _actions.Player.Use.started -= OnUseStarted;
        _actions.Player.Use.canceled -= OnUseCanceled;
        _actions.Player.Reload.performed -= OnReload;
        _actions.Player.SwapItem.performed -= OnSwapItem;
    }

    private void Update()
    {
        // Disparo automático para armas que lo sean
        if (!_isPressing) return;
        Item current = _equipService.CurrentItem;
        if (current is Weapon w && w.WeaponData != null && w.WeaponData.IsAutomatic)
            _equipService.UseCurrent();
    }

    private void OnUseStarted(InputAction.CallbackContext ctx)
    {
        _isPressing = true;
        Item current = _equipService.CurrentItem;
        if (current == null) return;

        // Para armas semi-automáticas o consumibles: un solo uso al pulsar
        bool isAutoWeapon = current is Weapon wp && wp.WeaponData != null && wp.WeaponData.IsAutomatic;
        if (!isAutoWeapon)
            _equipService.UseCurrent();
    }

    private void OnUseCanceled(InputAction.CallbackContext ctx) => _isPressing = false;

    private void OnReload(InputAction.CallbackContext ctx)
    {
        if (_equipService.CurrentItem is Weapon weapon)
            weapon.Reload();
    }

    private void OnSwapItem(InputAction.CallbackContext ctx)
    {
        float y = ctx.ReadValue<Vector2>().y;
        if (Mathf.Abs(y) > 0.01f)
            _equipService.SwapWithPrevious();
    }
}
