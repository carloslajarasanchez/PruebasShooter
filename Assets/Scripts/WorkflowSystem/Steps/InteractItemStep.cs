using System;
using UnityEngine.InputSystem;

public class InteractItemStep : IStep
{
    public string Name => "_tituloInteract";
    public string Description => "_tutoInteract";
    public bool IsCompleted { get => this._isCompleted; set => this._isCompleted = value; }
    public event Action OnCompleted;

    private ILogService _logService;
    private IPlayerInput _playerInput;
    private IAlertService _alertService;
    private IEventService _eventService;
    private bool _isCompleted = false;

    public InteractItemStep()
    {
        _logService = AppContainer.Get<ILogService>();
        _playerInput = AppContainer.Get<IPlayerInput>();
        _alertService = AppContainer.Get<IAlertService>();
        _eventService = AppContainer.Get<IEventService>();
    }

    public void Activate()
    {
        _logService.Add<InteractItemStep>($"Activando {this.Name}");
        _logService.Add<InteractItemStep>($"{this.Description}");
        _alertService.Show(this.Description, this.Name);
        this._playerInput.Actions.Player.Interact.performed += HandleAction;
    }

    public void Deactivate()
    {
        this._playerInput.Actions.Player.Interact.performed -= HandleAction;
    }

    private void HandleAction(InputAction.CallbackContext context)
    {
        //_eventService.Publish(new OnAlertMessageReceived(null, null));
        this.IsCompleted = true;
        this.OnCompleted?.Invoke();
    }
}
