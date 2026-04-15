using System;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;

public class EventService : IEventService
{
    /*private readonly Dictionary<string, Action> _events = new Dictionary<string, Action>();
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
    }*/

    //----------Avanzado----------
    
    private Dictionary<Type, List<Action<OwnEventBase>>> _events = new Dictionary<Type, List<Action<OwnEventBase>>>();
    public void Publish(OwnEventBase action)
    {
        var type = action.GetType();
        if (this._events.ContainsKey(type))
        {
            foreach(var handler in _events[type].ToArray())// Se convierte la lista a un array para evitar problemas de modificación durante la iteración
            {
                handler?.Invoke(action);
            }
        }
    }

    public void Subscribe<T>(Action<OwnEventBase> action)
    {
        Type type = typeof(T);
        if(!this._events.ContainsKey(type))
        {
            this._events[type] = new List<Action<OwnEventBase>>();
        }
        this._events[type].Add(action);
    }

    public void Unsubscribe<T>(Action<OwnEventBase> action)
    {
        Type type = action.GetType();
        if (this._events.ContainsKey(type))
        {
            this._events[type].Remove(action);
        }
    }
}