using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public abstract class Weapon : Item, IEquippable
{
    public bool IsReusable => true;
    public void OnPrimaryAction() => Shoot();

    public int CurrentAmmo { get; protected set; }
    public int MaxAmmo { get; protected set; }
    public bool IsReloading { get; protected set; }

    [Header("Weapon Data")]
    [SerializeField] private WeaponData _weaponData;
    public WeaponData WeaponData => _weaponData;

    [Header("References")]
    [SerializeField] private Transform _casingEjectPoint;
    [SerializeField] private Transform _muzzlePoint;

    [Header("Audio")]
    [SerializeField] private AudioClip _shootClip;
    [SerializeField] private AudioClip _reloadClip;
    [SerializeField] private AudioClip _emptyClip;

    [Header("VFX")]
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private GameObject _hitLight;

    private AudioSource _audioSource;
    private IEventService _eventService;
    private IPoolService _poolService;
    private ILogService _logService;
    private IInventoryService _inventory;        
    private float _nextFireTime;
    private bool _isEquipped;
    private Coroutine _reloadCoroutine;    

    protected new void Awake()
    {
        base.Awake();
        _audioSource = GetComponent<AudioSource>();
        _eventService = AppContainer.Get<IEventService>();
        _poolService = AppContainer.Get<IPoolService>();
        _logService = AppContainer.Get<ILogService>();
        _inventory = AppContainer.Get<IInventoryService>();

        if (_weaponData == null)
        {
            _logService.Add<Weapon>($"WeaponData no asignado en '{name}'");
            return;
        }

        MaxAmmo = _weaponData.MaxAmmo;
        CurrentAmmo = MaxAmmo;
    }

    // ── IEquippable ───────────────────────────────────────────────

    public virtual void OnEquipped()
    {
        _isEquipped = true;

        if (_lineRenderer != null)
        {
            _lineRenderer.enabled = true;
            _hitLight.SetActive(true);
        }

        _logService.Add<Weapon>($"'{Name}' equipada y lista para usar.");
    }

    public void OnUnequipped()
    {
        _isEquipped = false;

        // ── Cancelar recarga activa al desequipar ────────────────
        if (IsReloading)
        {
            CancelReload();
        }

        if (_lineRenderer != null)
        {
            _lineRenderer.enabled = false;
            _hitLight.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (!_isEquipped) return;
        UpdateLaser();
    }

    public override void Use() => Shoot();

    // ── Disparo ──────────────────────────────────────────────────

    public void Shoot()
    {
        if (IsReloading) return;
        if (Time.time < _nextFireTime) return;

        if (CurrentAmmo <= 0)
        {
            PlayClip(_emptyClip);
            return;
        }

        _nextFireTime = Time.time + _weaponData.FireRate;
        CurrentAmmo--;
        PlayClip(_shootClip);
        EjectCasing();
        PerformRaycast();
        SaveState();

        _eventService.Publish(new OnWeaponFired());
        _eventService.Publish(new OnAmmoChanged { CurrentAmmo = CurrentAmmo, MaxAmmo = MaxAmmo });

        if (CurrentAmmo == 0)
            StartCoroutine(AutoReloadCoroutine());
    }

    // ── Recarga con consumo de balas del inventario ───────────────

    public void Reload()
    {
        if (IsReloading || CurrentAmmo == MaxAmmo) return;

        IBullet bullets = _inventory?.GetItem<IBullet>(b => b.Type == _weaponData.WeaponType);

        if (bullets == null || bullets.BulletAmount <= 0)
        {
            _logService.Add<Weapon>($"'{Name}': sin balas de tipo {_weaponData.WeaponType} en el inventario.");
            return;
        }

        _reloadCoroutine = StartCoroutine(ReloadCoroutine(bullets));
    }

    private void CancelReload()
    {
        if (_reloadCoroutine != null)
        {
            StopCoroutine(_reloadCoroutine);
            _reloadCoroutine = null;
        }

        IsReloading = false;
        // Notificar a la UI que la recarga se canceló
        _eventService.Publish(new OnAmmoChanged { CurrentAmmo = CurrentAmmo, MaxAmmo = MaxAmmo });
        _logService.Add<Weapon>($"'{Name}': recarga cancelada al desequipar.");
    }

    // ── Raycast ───────────────────────────────────────────────────

    protected virtual void PerformRaycast()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        for (int i = 0; i < _weaponData.PelletCount; i++)
        {
            Vector3 dir = GetShotDirection(ray.direction);

            if (Physics.Raycast(ray.origin, dir, out RaycastHit hit, _weaponData.Range))
            {
                Debug.DrawLine(ray.origin, hit.point, Color.red, 1f);
                OnHit(hit);
            }
        }
    }

    // ── Láser ─────────────────────────────────────────────────────

    private void UpdateLaser()
    {
        if (_lineRenderer == null || _muzzlePoint == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.5f));
        RaycastHit hit;
        Vector3 endPoint;

        if (Physics.Raycast(ray.origin, ray.direction, out hit, _weaponData.Range, ~0, QueryTriggerInteraction.Ignore))
            endPoint = hit.point;
        else
            endPoint = ray.origin + ray.direction * _weaponData.Range;

        _hitLight.transform.position = hit.point;

        _lineRenderer.positionCount = 2;
        _lineRenderer.SetPosition(0, _muzzlePoint.position);
        _lineRenderer.SetPosition(1, endPoint);
    }

    protected virtual Vector3 GetShotDirection(Vector3 baseDirection)
    {
        if (_weaponData.SpreadAngle <= 0f) return baseDirection;
        float half = _weaponData.SpreadAngle * 0.5f;
        return Quaternion.Euler(
            Random.Range(-half, half),
            Random.Range(-half, half),
            0f
        ) * baseDirection;
    }

    protected virtual void OnHit(RaycastHit hit)
    {
        _logService.Add<Weapon>($"'{Name}' impactó '{hit.collider.name}' — daño: {_weaponData.Damage}");
    }

    // ── VFX ──────────────────────────────────────────────────────

    private void EjectCasing()
    {
        if (_weaponData.CasingPrefab == null || _casingEjectPoint == null) return;

        GameObject casing = _poolService.Get(
            _weaponData.CasingPrefab,
            _casingEjectPoint.position,
            _casingEjectPoint.rotation
        );

        if (casing.TryGetComponent<SkullCap>(out var skullCap))
            skullCap.Init(_weaponData.CasingPrefab);
    }

    // ── Audio ─────────────────────────────────────────────────────

    private void PlayClip(AudioClip clip)
    {
        if (clip == null) return;
        _audioSource.PlayOneShot(clip);
    }

    // ── Coroutines ────────────────────────────────────────────────

    private IEnumerator ReloadCoroutine(IBullet bullets)
    {
        IsReloading = true;
        PlayClip(_reloadClip);
        _eventService.Publish(new OnWeaponReloading { ReloadTime = _weaponData.ReloadTime });

        yield return new WaitForSeconds(_weaponData.ReloadTime);

        int needed = MaxAmmo - CurrentAmmo;
        int toLoad = Mathf.Min(needed, bullets.BulletAmount);

        bullets.BulletAmount -= toLoad;
        CurrentAmmo += toLoad;


        if (bullets is Item bulletItem)
            bulletItem.SaveState();

        if (bullets.BulletAmount <= 0)
            _inventory.RemoveItem(bullets as Item);

        IsReloading = false;
        _reloadCoroutine = null;

        _eventService.Publish(new OnWeaponReloaded { MaxAmmo = MaxAmmo });
        _eventService.Publish(new OnAmmoChanged { CurrentAmmo = CurrentAmmo, MaxAmmo = MaxAmmo });
    }

    private IEnumerator AutoReloadCoroutine()
    {
        yield return new WaitForSeconds(0.3f);
        Reload();
    }

    // ── Save / Load ───────────────────────────────────────────────

    public override void SaveState(bool? isConsumed = null, int? currentAmmo = null)
    {
        ItemState state = _saveService.GetOrCreateState<ItemState>(SaveId);
        state.isInInventory = _isInInventory;
        state.currentAmmo = currentAmmo ?? CurrentAmmo;
        _saveService.SetState(SaveId, state);
    }

    public override void RestoreState(ItemState state)
    {
        CurrentAmmo = state.currentAmmo;
        MaxAmmo = _weaponData != null ? _weaponData.MaxAmmo : MaxAmmo;
        base.RestoreState(state);

        _eventService.Publish(new OnAmmoChanged
        {
            CurrentAmmo = CurrentAmmo,
            MaxAmmo = MaxAmmo
        });
    }
}