using System;
using System.Collections.Generic;

public interface IEventService
{
    /*void Publish(string eventName);
    void Subscribe(string eventName, Action handler);
    void Unsubscribe(string eventName, Action handler);*/

    //----------MedioNivel----------
    //void Publish(string eventName, object data);
    //void Subscribe(string eventName, Action<object> handler);
    //void Unsubscribe(string eventName, Action<object> handler);

    //----------Avanzado----------
    public void Publish(OwnEventBase action);
    public void Subscribe<T>(Action<OwnEventBase> action);
    public void Unsubscribe<T>(Action<OwnEventBase> action);
}
