using System;

public interface IEventService
{
    public void Publish(OwnEventBase action);
    public void Subscribe<T>(Action<OwnEventBase> action);
    public void Unsubscribe<T>(Action<OwnEventBase> action);
}
