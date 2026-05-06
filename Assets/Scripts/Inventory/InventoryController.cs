using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryController : MonoBehaviour
{
    [SerializeField] private float _raycastDistance = 3f;
    [SerializeField] private LayerMask _catchableLayer;
    [SerializeField] private LayerMask _interactableLayer;

    private bool _isCatcheable = false;
    private ICatchable _itemToCatch;
    private bool _isDetecting;
    private PlayerInputActions _input;
    private SaveMachine _saveMachineToInteract;

    private IEventService _eventService;

    private OnCatchableDetected _catchableDetectedEvent;
    private OnCatchableLost _catchableLostEvent;

    private void OnEnable()
    {
        _catchableDetectedEvent = new OnCatchableDetected();
        _catchableLostEvent = new OnCatchableLost();
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
        bool hitCatchable = Physics.Raycast(Camera.main.ScreenPointToRay(_screenCenter), out RaycastHit hitCatchableResult, _raycastDistance, _catchableLayer);
        bool hitInteractable = Physics.Raycast(Camera.main.ScreenPointToRay(_screenCenter), out RaycastHit hitInteractableResult, _raycastDistance, _interactableLayer);
        Ray ray = Camera.main.ScreenPointToRay(_screenCenter);
        Debug.DrawRay(ray.origin, ray.direction * hitCatchableResult.distance, Color.green);

        ICatchable catchable = hitCatchable && hitCatchableResult.collider.TryGetComponent(out ICatchable found) ? found : null;
        SaveMachine saveMachine = hitInteractable && hitInteractableResult.collider.TryGetComponent(out SaveMachine foundMachine) ? foundMachine : null;

        if (catchable != null)
        {
            _itemToCatch = catchable;
            _saveMachineToInteract = null;

            if (!_isDetecting)
            {
                _isDetecting = true;
                _isCatcheable = true;
                _eventService.Publish(_catchableDetectedEvent);
            }
        }
        else if (saveMachine != null)
        {
            _itemToCatch = null;
            _saveMachineToInteract = saveMachine;

            if (!_isDetecting)
            {
                _isDetecting = true;
                _isCatcheable = true;
                _eventService.Publish(_catchableDetectedEvent);
            }
        }
        else
        {
            _itemToCatch = null;
            _saveMachineToInteract = null;

            if (_isDetecting)
            {
                _isDetecting = false;
                _isCatcheable = false;
                _eventService.Publish(_catchableLostEvent);
            }
        }
    }

    private void CatchItem(InputAction.CallbackContext context)
    {
        Debug.Log("Interact button pressed.");
        if (_isCatcheable)
        {
            if (_saveMachineToInteract != null)
            {
                Debug.Log("Opening save machine menu...");
                _saveMachineToInteract.OpenMenu();
            }
            else if (_itemToCatch != null)
            {
                Debug.Log("Attempting to catch item...");
                _itemToCatch.Catch();
            }
        }
    }
}
