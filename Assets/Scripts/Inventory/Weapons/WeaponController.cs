using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// MonoBehaviour que conecta el Input del jugador con el WeaponService.
/// Gestiona disparo (semi/auto), recarga y scroll entre armas.
/// 
/// ACCIONES A AÑADIR EN PlayerInputActions (mapa "Player"):
///   - Fire        → Button → Left Mouse Button
///   - Reload      → Button → R
///   - ScrollWeapon → Value → Vector2 → Mouse / Scroll
/// </summary>
public class WeaponController : MonoBehaviour
{
    [SerializeField] private Transform _hand; // Asignar en el Inspector al transform donde se equipan las armas  
    private IPlayerInput _playerInput;
    private IWeaponService _weaponService;
    private ILogService _logService;
    private PlayerInputActions _actions;

    private bool _isFiring;

    private void Awake()
    {
        _playerInput = AppContainer.Get<IPlayerInput>();
        _logService = AppContainer.Get<ILogService>();
        _weaponService = AppContainer.Get<IWeaponService>();

        if (_hand != null)
        {
            (_weaponService as WeaponService)?.SetHandTransform(_hand);
        }
        else
        {
            _logService.Add<WeaponController>($"[WeaponController] No se asignó el Transform de la mano en el Inspector. Asegúrate de arrastrar el transform correcto al campo '_hand' para que las armas se equipen correctamente.");
        }
    }

    private void OnEnable()
    {
        _actions = _playerInput.Actions;

        _actions.Player.Fire.started += OnFireStarted;
        _actions.Player.Fire.canceled += OnFireCanceled;
        _actions.Player.Reload.performed += OnReload;
        _actions.Player.ScrollWeapon.performed += OnScrollWeapon;
    }

    private void OnDisable()
    {
        _actions.Player.Fire.started -= OnFireStarted;
        _actions.Player.Fire.canceled -= OnFireCanceled;
        _actions.Player.Reload.performed -= OnReload;
        _actions.Player.ScrollWeapon.performed -= OnScrollWeapon;
    }

    private void Update()
    {
        // Disparo automático: mientras se mantiene el botón y el arma es automática
        if (!_isFiring) return;

        Weapon w = _weaponService.CurrentWeapon;
        if (w != null && w.WeaponData != null && w.WeaponData.IsAutomatic)
            w.Shoot();

    }

    // ── Callbacks ────────────────────────────────────────────────

    private void OnFireStarted(InputAction.CallbackContext ctx)
    {
        _isFiring = true;
        Weapon w = _weaponService.CurrentWeapon;
        if (w == null) return;

        // Semi-automática: dispara una sola vez al pulsar
        if (!w.WeaponData.IsAutomatic)
            w.Shoot();
    }

    private void OnFireCanceled(InputAction.CallbackContext ctx)
    {
        _isFiring = false;
    }

    private void OnReload(InputAction.CallbackContext ctx)
    {
        _weaponService.CurrentWeapon?.Reload();
    }

    private void OnScrollWeapon(InputAction.CallbackContext ctx)
    {
        float y = ctx.ReadValue<Vector2>().y;
        if (Mathf.Abs(y) > 0.01f)
            _weaponService.ScrollWeapon(y);
    }
}
