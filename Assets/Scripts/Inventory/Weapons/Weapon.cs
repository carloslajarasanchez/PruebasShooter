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
    [SerializeField] private float _rayDuration = 0.05f;

    // El AudioSource está en el propio GameObject del arma
    private AudioSource _audioSource;
    private IEventService _eventService;
    private float _nextFireTime;

    protected new void Awake()
    {
        base.Awake();
        _audioSource = GetComponent<AudioSource>();
        _eventService = AppContainer.Get<IEventService>();

        if (_weaponData == null)
        {
            Debug.LogError($"[Weapon] {name}: WeaponData no asignado.");
            return;
        }

        MaxAmmo = _weaponData.MaxAmmo;
        CurrentAmmo = MaxAmmo;
    }

    /// <summary>Llamado por EquipService cuando el arma se mueve a Hand.</summary>
    public void OnEquipped()
    {
        // El AudioSource ya está en este mismo GameObject, nada que buscar
        Debug.Log($"[Weapon] {Name} lista para usar.");
    }

    // ── IEquippable / Item ───────────────────────────────────────

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

        _eventService.Publish(new OnWeaponFired());
        _eventService.Publish(new OnAmmoChanged { CurrentAmmo = CurrentAmmo, MaxAmmo = MaxAmmo });

        if (CurrentAmmo == 0)
            StartCoroutine(AutoReloadCoroutine());
    }

    public void Reload()
    {
        if (IsReloading || CurrentAmmo == MaxAmmo) return;
        StartCoroutine(ReloadCoroutine());
    }

    // ── Raycast desde centro de pantalla ─────────────────────────

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
                DrawRay(hit.point);
                OnHit(hit);
            }
            else
            {
                DrawRay(ray.origin + dir * _weaponData.Range);
            }
        }
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
        Debug.Log($"[Weapon] '{Name}' impactó '{hit.collider.name}' — daño: {_weaponData.Damage}");
    }

    // ── VFX ──────────────────────────────────────────────────────

    private void EjectCasing()
    {
        if (_weaponData.CasingPrefab == null || _casingEjectPoint == null) return;

        GameObject casing = Instantiate(
            _weaponData.CasingPrefab,
            _casingEjectPoint.position,
            _casingEjectPoint.rotation
        );
    }

    // ── Audio ────────────────────────────────────────────────────

    private void PlayClip(AudioClip clip)
    {
        if (clip == null) return;
        _audioSource.PlayOneShot(clip);
    }

    // ── Coroutines ───────────────────────────────────────────────

    private IEnumerator ReloadCoroutine()
    {
        IsReloading = true;
        PlayClip(_reloadClip);
        _eventService.Publish(new OnWeaponReloading { ReloadTime = _weaponData.ReloadTime });

        yield return new WaitForSeconds(_weaponData.ReloadTime);

        CurrentAmmo = MaxAmmo;
        IsReloading = false;
        _eventService.Publish(new OnWeaponReloaded { MaxAmmo = MaxAmmo });
        _eventService.Publish(new OnAmmoChanged { CurrentAmmo = CurrentAmmo, MaxAmmo = MaxAmmo });
    }

    private IEnumerator AutoReloadCoroutine()
    {
        yield return new WaitForSeconds(0.3f);
        Reload();
    }
    private void DrawRay(Vector3 hitPoint)
    {
        if (_lineRenderer == null || _muzzlePoint == null) return;

        _lineRenderer.enabled = true;

        _lineRenderer.positionCount = 2;
        _lineRenderer.SetPosition(0, _muzzlePoint.position);
        _lineRenderer.SetPosition(1, hitPoint);

        StopCoroutine(nameof(DisableLine));
        StartCoroutine(DisableLine());
    }
    private IEnumerator DisableLine()
    {
        yield return new WaitForSeconds(_rayDuration);

        if (_lineRenderer != null)
            _lineRenderer.enabled = false;
    }
}