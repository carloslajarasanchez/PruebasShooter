using System;
using System.Threading.Tasks;
using UnityEditor.MPE;
using UnityEngine.InputSystem;

public class RunStep : IStep
{
    public string Name => "Control de correr";

    public string Description => "Manten el botón Shift para correr";

    public bool IsCompleted { get => this._isCompleted; set => this._isCompleted = value; }
    public event Action OnCompleted;

    private ILogService _logService;
    private IPlayerInput _playerInput;
    private IAlertService _alertService;
    private IEventService _eventService;
    private bool _isCompleted = false;

    public RunStep()
    {
        _logService = AppContainer.Get<ILogService>();
        _playerInput = AppContainer.Get<IPlayerInput>();
        _alertService = AppContainer.Get<IAlertService>();
        _eventService = AppContainer.Get<IEventService>();
    }

    public void Activate()
    {
        _logService.Add<RunStep>($"Activando {this.Name}");
        _logService.Add<RunStep>($"{this.Description}");
        _alertService.Show(this.Description, this.Name);
        this._playerInput.Actions.Player.Run.performed += HandleAction;
    }

    public void Deactivate()
    {
        this._playerInput.Actions.Player.Run.performed -= HandleAction;
    }

    private void HandleAction(InputAction.CallbackContext context)
    {
        //_eventService.Publish(new OnAlertMessageReceived(null, null));
        this.IsCompleted = true;
        this.OnCompleted?.Invoke();
    }
}
