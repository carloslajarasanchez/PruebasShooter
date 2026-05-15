using UnityEngine;
using TMPro;

public class WeaponAmmoUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _ammoPanel;
    [SerializeField] private TextMeshProUGUI _currentAmmoText;
    [SerializeField] private TextMeshProUGUI _maxAmmoText;
    [SerializeField] private TextMeshProUGUI _separatorText;

    [Header("Color Settings")]
    [SerializeField] private Color _fullAmmoColor = Color.white;
    [SerializeField] private Color _midAmmoColor = new Color(1f, 0.6f, 0f);
    [SerializeField] private Color _lowAmmoColor = Color.red;

    [Header("Thresholds (%)")]
    [SerializeField][Range(0f, 1f)] private float _lowAmmoThreshold = 0.25f;
    [SerializeField][Range(0f, 1f)] private float _midAmmoThreshold = 0.5f;

    private IEventService _eventService;
    private IEquipService _equipService;
    private IInventoryService _inventoryService;

    private void Awake()
    {
        _eventService = AppContainer.Get<IEventService>();
        _equipService = AppContainer.Get<IEquipService>();
        _inventoryService = AppContainer.Get<IInventoryService>();

        if (_ammoPanel != null)
            _ammoPanel.SetActive(false);
    }

    private void OnEnable()
    {
        _eventService.Subscribe<OnItemEquipped>(OnItemEquipped);
        _eventService.Subscribe<OnItemUnequipped>(OnItemUnequippedUI);
        _eventService.Subscribe<OnAmmoChanged>(OnAmmoChangedUI);
        _eventService.Subscribe<OnInventoryChanged>(OnInventoryChangedUI);
    }

    private void OnDisable()
    {
        _eventService.Unsubscribe<OnItemEquipped>(OnItemEquipped);
        _eventService.Unsubscribe<OnItemUnequipped>(OnItemUnequippedUI);
        _eventService.Unsubscribe<OnAmmoChanged>(OnAmmoChangedUI);
        _eventService.Unsubscribe<OnInventoryChanged>(OnInventoryChangedUI);
    }

    private void OnItemEquipped(OwnEventBase evt)
    {
        OnItemEquipped equipEvt = evt as OnItemEquipped;
        if (equipEvt.Item is Weapon weapon)
        {
            ShowAmmoPanel();
            RefreshAmmoDisplay();
        }
    }

    private void OnItemUnequippedUI(OwnEventBase evt)
    {
        OnItemUnequipped events = evt as OnItemUnequipped;
        if (events.Item is Weapon)
            HideAmmoPanel();
    }

    private void OnAmmoChangedUI(OwnEventBase evt)
    {
        RefreshAmmoDisplay();
    }

    private void OnInventoryChangedUI(OwnEventBase evt)
    {
        RefreshAmmoDisplay();
    }

    private void ShowAmmoPanel()
    {
        if (_ammoPanel != null)
            _ammoPanel.SetActive(true);
    }

    private void HideAmmoPanel()
    {
        if (_ammoPanel != null)
            _ammoPanel.SetActive(false);
    }

    private void RefreshAmmoDisplay()
    {
        if (_equipService.CurrentItem is not Weapon weapon) return;

        int totalReserve = GetTotalReserveAmmo(weapon.WeaponData.WeaponType);

        if (_currentAmmoText != null)
            _currentAmmoText.text = weapon.CurrentAmmo.ToString();

        if (_maxAmmoText != null)
            _maxAmmoText.text = totalReserve.ToString();

        Color targetColor = CalculateAmmoColor(weapon.CurrentAmmo, weapon.MaxAmmo);

        if (_currentAmmoText != null)
            _currentAmmoText.color = targetColor;

        if (_separatorText != null)
            _separatorText.color = targetColor;

        if (_maxAmmoText != null)
            _maxAmmoText.color = targetColor;
    }

    private int GetTotalReserveAmmo(WeaponTypeEnum weaponType)
    {
        int total = 0;
        foreach (Item item in _inventoryService.Items)
        {
            if (item is BulletBase bullet && bullet.Type == weaponType)
                total += bullet.BulletAmount;
        }
        return total;
    }

    private Color CalculateAmmoColor(int current, int max)
    {
        if (max == 0) return _lowAmmoColor;

        float percentage = (float)current / max;

        if (percentage <= _lowAmmoThreshold)
            return _lowAmmoColor;
        else if (percentage <= _midAmmoThreshold)
            return _midAmmoColor;
        else
            return _fullAmmoColor;
    }
}