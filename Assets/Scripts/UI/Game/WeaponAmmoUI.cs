using UnityEngine;
using TMPro;

/// <summary>
/// UI que muestra la munición actual del arma equipada.
/// Se suscribe a eventos de equipar/desequipar y cambios de munición.
/// El color cambia de blanco → naranja → rojo según las balas restantes.
/// </summary>
public class WeaponAmmoUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _ammoPanel;
    [SerializeField] private TextMeshProUGUI _currentAmmoText;
    [SerializeField] private TextMeshProUGUI _maxAmmoText;
    [SerializeField] private TextMeshProUGUI _separatorText; // El "/"

    [Header("Color Settings")]
    [SerializeField] private Color _fullAmmoColor = Color.white;
    [SerializeField] private Color _midAmmoColor = new Color(1f, 0.6f, 0f); // Naranja
    [SerializeField] private Color _lowAmmoColor = Color.red;

    [Header("Thresholds (%)")]
    [SerializeField][Range(0f, 1f)] private float _lowAmmoThreshold = 0.25f;   // 25% o menos = rojo
    [SerializeField][Range(0f, 1f)] private float _midAmmoThreshold = 0.5f;    // 50% o menos = naranja

    private IEventService _eventService;
    private IEquipService _equipService;

    private void Awake()
    {
        _eventService = AppContainer.Get<IEventService>();
        _equipService = AppContainer.Get<IEquipService>();

        // Ocultar panel al inicio
        if (_ammoPanel != null)
            _ammoPanel.SetActive(false);
    }

    private void OnEnable()
    {
        _eventService.Subscribe<OnItemEquipped>(OnItemEquipped);
        _eventService.Subscribe<OnItemUnequipped>(OnItemUnequippedUI);
        _eventService.Subscribe<OnAmmoChanged>(OnAmmoChangedUI);
    }

    private void OnDisable()
    {
        _eventService.Unsubscribe<OnItemEquipped>(OnItemEquipped);
        _eventService.Unsubscribe<OnItemUnequipped>(OnItemUnequippedUI);
        _eventService.Unsubscribe<OnAmmoChanged>(OnAmmoChangedUI);
    }

    private void OnItemEquipped(OwnEventBase evt)
    {
        OnItemEquipped equipEvt = evt as OnItemEquipped;
        // Solo mostrar UI si el item equipado es un arma
        if (equipEvt.Item is Weapon weapon)
        {
            ShowAmmoPanel();
            UpdateAmmoDisplay(weapon.CurrentAmmo, weapon.MaxAmmo);
        }
    }

    private void OnItemUnequippedUI(OwnEventBase evt)
    {
        OnItemUnequipped events = evt as OnItemUnequipped;
        // Ocultar UI cuando se desequipa cualquier item
        // (por si acaso se desequipa un arma)
        if (events.Item is Weapon)
        {
            HideAmmoPanel();
        }
    }

    private void OnAmmoChangedUI(OwnEventBase evt)
    {
        OnAmmoChanged events = evt as OnAmmoChanged;
        // Actualizar los números y el color
        UpdateAmmoDisplay(events.CurrentAmmo, events.MaxAmmo);
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

    private void UpdateAmmoDisplay(int currentAmmo, int maxAmmo)
    {
        if (_currentAmmoText != null)
            _currentAmmoText.text = currentAmmo.ToString();

        if (_maxAmmoText != null)
            _maxAmmoText.text = maxAmmo.ToString();

        // Calcular color según el porcentaje de munición
        Color targetColor = CalculateAmmoColor(currentAmmo, maxAmmo);

        if (_currentAmmoText != null)
            _currentAmmoText.color = targetColor;

        if (_separatorText != null)
            _separatorText.color = targetColor;

        // El máximo siempre en blanco o también con el color
        // (puedes comentar esta línea si prefieres que maxAmmo no cambie de color)
        if (_maxAmmoText != null)
            _maxAmmoText.color = targetColor;
    }

    private Color CalculateAmmoColor(int current, int max)
    {
        if (max == 0) return _lowAmmoColor;

        float percentage = (float)current / max;

        if (percentage <= _lowAmmoThreshold)
        {
            return _lowAmmoColor; // Rojo
        }
        else if (percentage <= _midAmmoThreshold)
        {
            return _midAmmoColor; // Naranja
        }
        else
        {
            return _fullAmmoColor; // Blanco
        }
    }
}