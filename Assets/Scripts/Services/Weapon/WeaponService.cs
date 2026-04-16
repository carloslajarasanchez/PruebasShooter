using System.Collections.Generic;
using UnityEngine;

public class WeaponService : IWeaponService
{
    public Weapon CurrentWeapon { get; private set; }
    public List<Weapon> EquippedWeapons { get; private set; } = new List<Weapon>();

    private int _currentIndex = -1;
    private IEventService _eventService;
    private ILogService _logService;
    private Transform _handTransform;
    private GameObject _currentWeaponModel;
    private int _itemLayerMask;

    public WeaponService()
    {
        _eventService = AppContainer.Get<IEventService>();
        _logService = AppContainer.Get<ILogService>();
        _itemLayerMask = LayerMask.NameToLayer("Hand");

        /*GameObject hand = GameObject.FindWithTag("Hand");
        if (hand != null)
        {
            _handTransform = hand.transform;
        }
        else
        {
            _logService.Add<WeaponService>("[WeaponService] No se encontró el GameObject con tag 'Hand'. Asegúrate de que el jugador tiene un hijo con ese tag para equipar las armas.");
        }*/

        if (_itemLayerMask == -1)
        {
            _logService.Add<WeaponService>("[WeaponService] No se encontró el layer 'Item'. Asegúrate de que existe y que los modelos de las armas están asignados a ese layer.");
        }
    }

    public void SetHandTransform(Transform hand) => _handTransform = hand;

    /// <summary>
    /// Equipa un arma. Si ya está equipada, la activa. Si no, la añade a la lista.
    /// </summary>
    public void EquipWeapon(Weapon weapon)
    {
        int existingIndex = EquippedWeapons.IndexOf(weapon);

        if (existingIndex >= 0)
        {
            // Ya la tenemos, solo cambiamos a ella
            SwitchToIndex(existingIndex);
            return;
        }

        EquippedWeapons.Add(weapon);
        SwitchToIndex(EquippedWeapons.Count - 1);
    }

    public void UnequipAll()
    {
        DestroyCurrentModel();
        EquippedWeapons.Clear();
        CurrentWeapon = null;
        _currentIndex = -1;
        _eventService.Publish(new OnWeaponChanged { Weapon = null, CurrentAmmo = 0, MaxAmmo = 0 });
    }

    /// <summary>
    /// Cambia de arma con la rueda del ratón. direction > 0 = siguiente, < 0 = anterior.
    /// </summary>
    public void ScrollWeapon(float direction)
    {
        if (EquippedWeapons.Count <= 1) return;

        int next = _currentIndex + (direction > 0 ? 1 : -1);
        next = ((next % EquippedWeapons.Count) + EquippedWeapons.Count) % EquippedWeapons.Count;
        SwitchToIndex(next);
    }

    private void SwitchToIndex(int index)
    {
        DestroyCurrentModel();

        _currentIndex = index;
        CurrentWeapon = EquippedWeapons[_currentIndex];

        SpawnWeaponModel(CurrentWeapon);
        _eventService.Publish(new OnWeaponChanged { Weapon = CurrentWeapon, CurrentAmmo = CurrentWeapon.CurrentAmmo, MaxAmmo = CurrentWeapon.MaxAmmo });

        _logService.Add<WeaponService>($"[WeaponService] Cambió a arma '{CurrentWeapon.Name}' (índice {_currentIndex})");
    }

    private void SpawnWeaponModel(Weapon weapon)
    {
        if (_handTransform == null)
        {
            _logService.Add<WeaponService>("[WeaponService] No se puede equipar el arma porque no se ha asignado el Transform de la mano.");
            return;
        }

        GameObject prefab = weapon.ModelPrefab;
        if (prefab == null)
        {
            _logService.Add<WeaponService>($"[WeaponService] El arma '{weapon.Name}' no tiene un ModelPrefab asignado en su WeaponData.");
            return;
        }

        _currentWeaponModel = Object.Instantiate(prefab, _handTransform.position, _handTransform.rotation, _handTransform);
        if(_itemLayerMask != -1)
            SetLayerRecursively(_currentWeaponModel, _itemLayerMask);
    }

    private void DestroyCurrentModel()
    {
        if (_currentWeaponModel != null)
        {
            Object.Destroy(_currentWeaponModel);
            _currentWeaponModel = null;
        }
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}
