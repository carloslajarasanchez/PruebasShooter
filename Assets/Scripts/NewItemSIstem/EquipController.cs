using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controlador unificado de equipamiento.
/// Pon este script en el GameObject Player.
///
/// ACCIONES en el mapa "Player" de PlayerInputActions:
///   Use      → Button → Left Mouse Button
///   Reload   → Button → R
///   SwapItem → Value, Vector2 → Mouse/Scroll
/// </summary>
public class EquipController : MonoBehaviour
{
    [SerializeField] private Transform _itemStorage; // Empty hijo del Player
    [SerializeField] private Transform _hand;        // Empty hijo de Camera

    private IPlayerInput _playerInput;
    private IEquipService _equipService;
    private PlayerInputActions _actions;
    private bool _isPressing;

    private void Awake()
    {
        _playerInput = AppContainer.Get<IPlayerInput>();
        _equipService = AppContainer.Get<IEquipService>();
        _equipService.SetTransforms(_itemStorage, _hand);
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
        if (!_isPressing) return;
        if (_equipService.CurrentItem is Weapon w && w.WeaponData != null && w.WeaponData.IsAutomatic)
            _equipService.UseCurrent();
    }

    private void OnUseStarted(InputAction.CallbackContext ctx)
    {
        _isPressing = true;
        Item current = _equipService.CurrentItem;
        if (current == null) return;

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