using System.Collections;
using UnityEngine;

/// <summary>
/// Clase base para armas. Implementa IEquippable: OnPrimaryAction() dispara.
/// El raycast sale del centro de la pantalla, no del muzzle, para comportamiento
/// FPS estándar. El muzzle solo se usa para efectos visuales (vainas, partículas).
///
/// IMPORTANTE — Audio:
/// El AudioSource debe estar en el ModelPrefab (el prefab de la mano), NO en el
/// prefab del suelo. Se obtiene en OnModelSpawned(), que EquipService llama
/// tras instanciar el modelo. Si el AudioSource está en el prefab del suelo
/// (que se destruye en Catch) no habrá componente donde reproducir el sonido.
/// </summary>
public abstract class Weapon : Item, IEquippable
{
    // ── IEquippable ──────────────────────────────────────────────
    public bool IsReusable => true;
    public void OnPrimaryAction() => Shoot();

    // ── Stats ────────────────────────────────────────────────────
    public int CurrentAmmo { get; protected set; }
    public int MaxAmmo { get; protected set; }
    public bool IsReloading { get; protected set; }

    [Header("Weapon Data")]
    [SerializeField] private WeaponData _weaponData;
    public WeaponData WeaponData => _weaponData;

    [Header("Audio clips")]
    [SerializeField] private AudioClip _shootClip;
    [SerializeField] private AudioClip _reloadClip;
    [SerializeField] private AudioClip _emptyClip;

    // AudioSource vive en el ModelPrefab instanciado en Hand, no en este GameObject.
    // Se asigna cuando EquipService instancia el modelo.
    private AudioSource _audioSource;
    private IEventService _eventService;
    private float _nextFireTime;

    protected new void Awake()
    {
        base.Awake();
        _eventService = AppContainer.Get<IEventService>();

        if (_weaponData == null)
        {
            Debug.LogError($"[Weapon] {name}: WeaponData no asignado.");
            return;
        }

        MaxAmmo = _weaponData.MaxAmmo;
        CurrentAmmo = MaxAmmo;
    }

    /// <summary>
    /// Llamado por EquipService tras instanciar el ModelPrefab en Hand.
    /// Le pasamos el GameObject del modelo para obtener su AudioSource.
    /// </summary>
    public void OnModelSpawned(GameObject model)
    {
        _audioSource = model.GetComponentInChildren<AudioSource>();
        if (_audioSource == null)
            Debug.LogWarning($"[Weapon] '{Name}': el ModelPrefab no tiene AudioSource. No habrá audio.");
    }

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

        Ray screenRay = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        int pellets = _weaponData.PelletCount;
        for (int i = 0; i < pellets; i++)
        {
            Ray ray = new Ray(screenRay.origin, GetShotDirection(screenRay.direction));

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
        if (_weaponData.CasingPrefab == null) return;

        // El punto de eyección vive en el modelo instanciado en Hand.
        // Lo buscamos por tag o nombre en los hijos del modelo actual.
        Transform eject = _audioSource?.transform.parent
            .GetComponentInChildren<CasingEjectPoint>()?.transform;

        if (eject == null) return;

        GameObject casing = Instantiate(_weaponData.CasingPrefab, eject.position, eject.rotation);
        Destroy(casing, 5f);
    }

    // ── Audio ────────────────────────────────────────────────────

    private void PlayClip(AudioClip clip)
    {
        if (clip == null) return;
        if (_audioSource == null)
        {
            Debug.LogWarning($"[Weapon] '{Name}': AudioSource no disponible. ¿Se llamó OnModelSpawned?");
            return;
        }
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
}
