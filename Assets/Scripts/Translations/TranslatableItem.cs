using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TranslatableItem : MonoBehaviour
{
    [SerializeField] private string _key;
    public string Key
    {
        get => _key;
        set
        {
            _key = value;
            UpdateText(null);
        }
    }

    private TextMeshProUGUI _text;
    private IEventService _eventService;
    private ITranslationService _translationService; // Cacheado

    private void Awake()
    {
        _eventService = AppContainer.Get<IEventService>();
        _translationService = AppContainer.Get<ITranslationService>();
        _text = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        UpdateText(null);
    }

    private void OnEnable()
    {
        _eventService.Subscribe<OnLanguageChanged>(UpdateText);
    }

    private void OnDisable()
    {
        _eventService.Unsubscribe<OnLanguageChanged>(UpdateText);
    }

    private void UpdateText(OwnEventBase parameters)
    {
        if (_text == null || _translationService == null) return;
        _text.text = _translationService.Get(_key);
    }
}