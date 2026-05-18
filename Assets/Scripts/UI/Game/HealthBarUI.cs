using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image _healthImage;
    [SerializeField] private TextMeshProUGUI _healthText;

    [Header("Colores del texto")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _warningColor = new Color(1f, 0.6f, 0f);
    [SerializeField] private Color _dangerColor = Color.red;

    private IEventService _eventService;

    private void Awake()
    {
        _eventService = AppContainer.Get<IEventService>();
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

        _healthImage.fillAmount = healthPercent;

        if (_healthText != null)
        {
            _healthText.text = currentLives.ToString();

            Color targetColor = healthPercent > 0.5f ? _normalColor : healthPercent > 0.2f ? _warningColor : _dangerColor;
            _healthText.color = targetColor;
        }
    }
}
