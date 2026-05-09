using System;
using System.Threading.Tasks;

public class EquipItemStep : IStep
{
    public string Name => "Equipar objetos";

    public string Description => "Haz clic en el boton de equipar para equiparlo";

    public bool IsCompleted { get => this._isCompleted; set => this._isCompleted = value; }
    public event Action OnCompleted;

    private ILogService _logService;
    private IAlertService _alertService;
    private IEventService _eventService;
    private bool _isCompleted = false;

    public EquipItemStep()
    {
        _logService = AppContainer.Get<ILogService>();
        _alertService = AppContainer.Get<IAlertService>();
        _eventService = AppContainer.Get<IEventService>();
    }

    public void Activate()
    {
        _logService.Add<EquipItemStep>($"Activando {this.Name}");
        _logService.Add<EquipItemStep>($"{this.Description}");
        _alertService.Show(this.Description, this.Name);
        _eventService.Subscribe<OnFirstEquipItem>(HandleAction);
    }

    public void Deactivate()
    {
        _eventService.Unsubscribe<OnFirstEquipItem>(HandleAction);
    }

    private void HandleAction(OwnEventBase parameters)
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
