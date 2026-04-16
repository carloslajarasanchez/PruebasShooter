using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public abstract class Weapon : Item, IWeapon, IInventory
{
    // ── IInventory ───────────────────────────────────────────────
    public bool IsReusable { get; set; } = true;

    // ── IWeapon ──────────────────────────────────────────────────
    public int CurrentAmmo { get; protected set; }
    public int MaxAmmo { get; protected set; }
    public bool IsReloading { get; protected set; }
    public float FireRate { get; protected set; }

    [Header("Weapon Data")]
    [SerializeField] private WeaponData _weaponData;

    [Header("References")]
    [SerializeField] protected Transform _muzzlePoint;
    [SerializeField] protected Transform _casingEjectPoint;

    [Header("Audio")]
    [SerializeField] private AudioClip _shootClip;
    [SerializeField] private AudioClip _reloadClip;
    [SerializeField] private AudioClip _emptyClip;

    private AudioSource _audioSource;
    private IEventService _eventService;
    private float _nextFireTime;

    public WeaponData WeaponData => _weaponData;

    private OnWeaponFired _onWeaponFired = new OnWeaponFired();

    protected new void Awake()
    {
        base.Awake();
        _eventService = AppContainer.Get<IEventService>();
        _audioSource = GetComponent<AudioSource>();

        if (_weaponData == null)
        {
            Debug.LogError($"[Weapon] {name}: WeaponData no asignado en el Inspector.");
            return;
        }

        MaxAmmo = _weaponData.MaxAmmo;
        CurrentAmmo = MaxAmmo;
        FireRate = _weaponData.FireRate;
    }

    // ── Item overrides ───────────────────────────────────────────

    /// <summary>Use() en armas dispara. No llama base.Use() para no eliminar el item del inventario.</summary>
    public override void Use() => Shoot();

    public override void Equip()
    {
        Debug.Log($"[Weapon] {Name} equipada.");
        AppContainer.Get<IWeaponService>().EquipWeapon(this);
    }

    // ── IWeapon ──────────────────────────────────────────────────

    public void Shoot()
    {
        if (IsReloading) return;
        if (Time.time < _nextFireTime) return;

        if (CurrentAmmo <= 0)
        {
            PlayClip(_emptyClip);
            return;
        }

        _nextFireTime = Time.time + FireRate;
        CurrentAmmo--;
        PlayClip(_shootClip);
        EjectCasing();
        PerformRaycast();

        //AppContainer.Get<IEventService>().Publish(GameEvents.OnWeaponFired);
        _eventService.Publish(_onWeaponFired);
        _eventService.Publish(new OnAmmoChanged { CurrentAmmo = CurrentAmmo, MaxAmmo = MaxAmmo });

        if (CurrentAmmo == 0)
            StartCoroutine(AutoReloadCoroutine());
    }

    public void Reload()
    {
        if (IsReloading || CurrentAmmo == MaxAmmo) return;
        StartCoroutine(ReloadCoroutine());
    }

    // ── Raycast ──────────────────────────────────────────────────

    protected virtual void PerformRaycast()
    {
        int pellets = _weaponData.PelletCount;
        for (int i = 0; i < pellets; i++)
        {
            Vector3 dir = GetShotDirection();
            Ray ray = new Ray(_muzzlePoint.position, dir);

            if (Physics.Raycast(ray, out RaycastHit hit, _weaponData.Range))
            {
                Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, 0.5f);
                OnHit(hit);
            }
            else
            {
                Debug.DrawRay(ray.origin, ray.direction * _weaponData.Range, Color.yellow, 0.5f);
            }
        }
    }

    protected virtual Vector3 GetShotDirection()
    {
        if (_weaponData.SpreadAngle <= 0f)
            return _muzzlePoint.forward;

        float half = _weaponData.SpreadAngle * 0.5f;
        return Quaternion.Euler(
            Random.Range(-half, half),
            Random.Range(-half, half),
            0f
        ) * _muzzlePoint.forward;
    }

    /// <summary>Sobreescribir para aplicar daño, efectos de impacto, etc.</summary>
    protected virtual void OnHit(RaycastHit hit)
    {
        Debug.Log($"[Weapon] {Name} impactó '{hit.collider.name}' a {hit.distance:F1}m — daño: {_weaponData.Damage}");
    }

    // ── VFX ──────────────────────────────────────────────────────

    private void EjectCasing()
    {
        if (_weaponData.CasingPrefab == null || _casingEjectPoint == null) return;
        GameObject casing = Instantiate(_weaponData.CasingPrefab, _casingEjectPoint.position, _casingEjectPoint.rotation);
        Destroy(casing, 5f);
    }

    // ── Coroutines ───────────────────────────────────────────────

    private IEnumerator ReloadCoroutine()
    {
        IsReloading = true;
        PlayClip(_reloadClip);
        //AppContainer.Get<IEventService>().Publish(GameEvents.OnWeaponReloading);
        _eventService.Publish(new OnWeaponReloading { ReloadTime = _weaponData.ReloadTime });

        yield return new WaitForSeconds(_weaponData.ReloadTime);

        CurrentAmmo = MaxAmmo;
        IsReloading = false;
        //AppContainer.Get<IEventService>().Publish(GameEvents.OnWeaponReloaded);
        _eventService.Publish(new OnWeaponReloaded { MaxAmmo = MaxAmmo});
        _eventService.Publish(new OnAmmoChanged { CurrentAmmo = CurrentAmmo, MaxAmmo = MaxAmmo });
    }

    private IEnumerator AutoReloadCoroutine()
    {
        yield return new WaitForSeconds(0.3f);
        Reload();
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip != null && _audioSource != null)
            _audioSource.PlayOneShot(clip);
    }
}