using System;

public interface IEventService
{
    void Publish(string eventName);
    void Subscribe(string eventName, Action handler);
    void Unsubscribe(string eventName, Action handler);
}
