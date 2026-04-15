using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryController : MonoBehaviour
{
    [SerializeField] private float _raycastDistance = 3f;
    [SerializeField] private LayerMask _catchableLayer;

    private bool _isCatcheable = false;
    private ICatchable _itemToCatch;
    private bool _isDetecting;
    private PlayerInputActions _input;

    private IEventService _eventService;

    private OnCatchableDetected _catchableDetectedEvent = new OnCatchableDetected();
    private OnCatchableLost _catchableLostEvent = new OnCatchableLost();

    private void OnEnable()
    {
        _input = AppContainer.Get<IPlayerInput>().Actions;
        _input.Player.Interact.performed += CatchItem;
        _eventService = AppContainer.Get<IEventService>();
    }

    private void OnDisable()
    {
        if (_input == null) return;

        _input.Player.Interact.performed -= CatchItem;
    }

    private void Update()
    {
        CheckObject();
    }

    private void CheckObject()
    {
        Vector2 _screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        bool hitSomething = Physics.Raycast(Camera.main.ScreenPointToRay(_screenCenter), out RaycastHit hit, _raycastDistance, _catchableLayer);
        Ray ray = Camera.main.ScreenPointToRay(_screenCenter);
        Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green);

        ICatchable catchable = hitSomething && hit.collider.TryGetComponent(out ICatchable found) ? found : null;

        if (catchable != null)
        {
            _itemToCatch = catchable;

            if (!_isDetecting)
            {
                _isDetecting = true;
                _isCatcheable = true;
                //_eventService.Publish(GameEvents.OnCatchableDetected);
                _eventService.Publish(_catchableDetectedEvent);
            }
        }
        else
        {
            _itemToCatch = null;

            if (_isDetecting)
            {
                _isDetecting = false;
                _isCatcheable = false;
                //_eventService.Publish(GameEvents.OnCatchableLost);
                _eventService.Publish(_catchableLostEvent);
            }
        }
    }

    private void CatchItem(InputAction.CallbackContext context)
    {
        Debug.Log("Interact button pressed.");
        if (_isCatcheable)
        {
            Debug.Log("Attempting to catch item...");
            _itemToCatch.Catch();
        }  
    }
}
