using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    //[SerializeField] private Image _healthImage;
    [SerializeField] private Slider _healthSlider;

    [Header("Colores del slider")]
    [SerializeField] private Color _normalColor = Color.green;
    [SerializeField] private Color _warningColor = new Color(1f, 0.6f, 0f);
    [SerializeField] private Color _dangerColor = Color.red;

    private IEventService _eventService;
    private Image _sliderFillImage;

    private void Awake()
    {
        _eventService = AppContainer.Get<IEventService>();

        // Obtener la imagen de relleno del slider
        if (_healthSlider != null)
        {
            _sliderFillImage = _healthSlider.fillRect.GetComponent<Image>();
        }
    }

    private void Start()
    {
        IPlayer player = AppContainer.Get<IPlayer>();
        SetHealthUI(player.Lives, 100);
    }

    private void OnEnable()
    {
        _eventService.Subscribe<OnLivesChanged>(OnLivesChanged);
    }

    private void OnDisable()
    {
        _eventService.Unsubscribe<OnLivesChanged>(OnLivesChanged);
    }

    private void OnLivesChanged(OwnEventBase parameters)
    {
        if (parameters is OnLivesChanged evt)
        {
            SetHealthUI(evt.CurrentLives, evt.MaxLives);
        }
    }

    private void SetHealthUI(int currentLives, int maxLives)
    {
        float healthPercent = currentLives / (float)maxLives;

        // Actualizar la imagen antigua (por si la quieres mantener)
        /*if (_healthImage != null)
        {
            _healthImage.fillAmount = healthPercent;
        }*/

        // Actualizar el slider
        if (_healthSlider != null)
        {
            _healthSlider.maxValue = maxLives;
            _healthSlider.value = currentLives;

            // Cambiar color según el porcentaje de vida
            if (_sliderFillImage != null)
            {
                Color targetColor = healthPercent > 0.5f ? _normalColor
                                  : healthPercent > 0.2f ? _warningColor
                                  : _dangerColor;
                _sliderFillImage.color = targetColor;
            }
        }
    }
}