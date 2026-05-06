public class AlertService : IAlertService
{
    private readonly IEventService _eventService;

    public AlertService()
    {
        // Obtenemos el servicio de eventos del contenedor[cite: 1]
        _eventService = AppContainer.Get<IEventService>();
    }

    public void Show(string description, string title = null)
    {
        // Publicamos el evento para que la UI lo recoja[cite: 5]
        _eventService.Publish(new OnAlertMessageReceived(description, title));
    }
}
