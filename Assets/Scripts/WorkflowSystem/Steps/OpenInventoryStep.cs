using System;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

public class OpenInventoryStep : IStep
{
    public string Name => "Abrir inventario";

    public string Description => "Presiona el boton Tab para abrir y cerrar el inventario";

    public bool IsCompleted { get => this._isCompleted; set => this._isCompleted = value; }
    public event Action OnCompleted;

    private ILogService _logService;
    private IPlayerInput _playerInput;
    private IAlertService _alertService;
    private IEventService _eventService;
    private bool _isCompleted = false;

    public OpenInventoryStep()
    {
        _logService = AppContainer.Get<ILogService>();
        _playerInput = AppContainer.Get<IPlayerInput>();
        _alertService = AppContainer.Get<IAlertService>();
        _eventService = AppContainer.Get<IEventService>();
    }

    public void Activate()
    {
        _logService.Add<OpenInventoryStep>($"Activando {this.Name}");
        _logService.Add<OpenInventoryStep>($"{this.Description}");
        _alertService.Show(this.Description, this.Name);
        this._playerInput.Actions.Player.Inventory.performed += HandleAction;
    }

    public void Deactivate()
    {
        this._playerInput.Actions.Player.Inventory.performed -= HandleAction;
    }

    private void HandleAction(InputAction.CallbackContext context)
    {
        CompleteAfterDelay();
    }

    private async void CompleteAfterDelay()
    {
        await Task.Delay(TimeSpan.FromSeconds(2f));
        _eventService.Publish(new OnAlertMessageReceived(null, null));
        await Task.Delay(TimeSpan.FromSeconds(2f));
        this.IsCompleted = true;
        this.OnCompleted?.Invoke();
    }
}
