using UnityEngine;
using UnityEngine.UI;

public class ItemDetector : MonoBehaviour
{
    [SerializeField] private Sprite _handSprite;

    private Image _icon;
    private Sprite _defaultSprite;

    private IEventService _eventService;

    void Awake()
    {
        _icon = GetComponent<Image>();
        _defaultSprite = _icon.sprite;
        _eventService = AppContainer.Get<IEventService>();

    }

    private void OnEnable()
    {
        //_eventService.Subscribe(GameEvents.OnCatchableDetected, DetectedItem);
        //_eventService.Subscribe(GameEvents.OnCatchableLost, LostItem);
        _eventService.Subscribe<OnCatchableDetected>(DetectedItem);
        _eventService.Subscribe<OnCatchableLost>(LostItem);
    }

    private void OnDisable()
    {
        //_eventService.Unsubscribe(GameEvents.OnCatchableDetected, DetectedItem);
        //_eventService.Unsubscribe(GameEvents.OnCatchableLost, LostItem);
        _eventService.Unsubscribe<OnCatchableDetected>(DetectedItem);
        _eventService.Unsubscribe<OnCatchableLost>(LostItem);
    }

    private void DetectedItem(OwnEventBase action)
    {
        _icon.sprite = _handSprite;
    }

    private void LostItem(OwnEventBase action)
    {
        _icon.sprite = _defaultSprite;
    }
}
