using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ItemDetector : MonoBehaviour
{
    [SerializeField] private Sprite _handSprite;

    private Image _icon;
    private RectTransform _rectTransform;
    private Sprite _defaultSprite;

    private IEventService _eventService;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _icon = GetComponent<Image>();
        _defaultSprite = _icon.sprite;
        _eventService = AppContainer.Get<IEventService>();
        
    }

    private void OnEnable()
    {
        _eventService.Subscribe(GameEvents.OnCatchableDetected, DetectedItem);
        _eventService.Subscribe(GameEvents.OnCatchableLost, LostItem);
    }

    private void OnDisable()
    {
        _eventService.Unsubscribe(GameEvents.OnCatchableDetected, DetectedItem);
        _eventService.Unsubscribe(GameEvents.OnCatchableLost, LostItem);
    }

    private void DetectedItem()
    {
        //_rectTransform.localScale = Vector3.one * 2.5f;
        _icon.sprite = _handSprite;
    }

    private void LostItem()
    {
        //_rectTransform.localScale = Vector3.one;
        _icon.sprite = _defaultSprite;
    }
}
