using System;
using System.Collections.Generic;

public class EventService : IEventService
{
    private readonly Dictionary<string, Action> _events = new Dictionary<string, Action>();
    private readonly ILogService _logService;

    public EventService()
    {
        _logService = AppContainer.Get<ILogService>();
    }

    public void Publish(string eventName)
    {
        if (!_events.TryGetValue(eventName, out Action handler))
        {
            _events[eventName] = null;
            _logService.Add<EventService>($"Publish: `{eventName}` no tenía suscriptores, registrado.");
            return;
        }

        handler?.Invoke();
        _logService.Add<EventService>($"Publish: `{eventName}` invocado.");
    }

    public void Subscribe(string eventName, Action handler)
    {
        if (!_events.ContainsKey(eventName))
        {
            _events[eventName] = null;
            _logService.Add<EventService>($"Subscribe: `{eventName}` registrado automáticamente.");
        }

        _events[eventName] += handler;
    }

    public void Unsubscribe(string eventName, Action handler)
    {
        if (!_events.ContainsKey(eventName))
        {
            _logService.Add<EventService>($"Unsubscribe: `{eventName}` no existe.");
            return;
        }

        _events[eventName] -= handler;
    }
}