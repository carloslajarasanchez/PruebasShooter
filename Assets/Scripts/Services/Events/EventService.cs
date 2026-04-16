using System;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;

public class EventService : IEventService
{
    private Dictionary<Type, List<Action<OwnEventBase>>> _events = new Dictionary<Type, List<Action<OwnEventBase>>>();
    public void Publish(OwnEventBase action)
    {
        var type = action.GetType();
        if (this._events.ContainsKey(type))
        {
            foreach (var handler in _events[type].ToArray())// Se convierte la lista a un array para evitar problemas de modificación durante la iteración
            {
                handler?.Invoke(action);
            }
        }
    }

    public void Subscribe<T>(Action<OwnEventBase> action)
    {
        Type type = typeof(T);
        if (!this._events.ContainsKey(type))
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