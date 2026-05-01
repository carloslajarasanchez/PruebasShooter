using System.Collections.Generic;

public class GameState : IGameState
{
    private Dictionary<string, bool> flags = new();
    private Dictionary<string, bool> triggers = new();
    private IEventService _events;
    public Dictionary<string, bool> GetFlags() => new(flags);
    public Dictionary<string, bool> GetTriggers() => new(triggers);
    public GameState()
    {
        _events = AppContainer.Get<IEventService>();
    }

    public void SetFlags(Dictionary<string, bool> newFlags)
    {
        flags = newFlags;

        foreach (var kvp in flags)
        {
            _events.Publish(new OnFlagChangedEvent
            {
                Key = kvp.Key,
                Value = kvp.Value
            });
        }
    }
    public bool GetTrigger(string key)
    {
        return triggers.TryGetValue(key, out var value) && value;
    }
    public void SetTriggers(Dictionary<string, bool> newTriggers)
    {
        triggers = newTriggers;
    }

    public bool GetFlag(string key)
    {
        return flags.TryGetValue(key, out var value) && value;
    }

    public void SetFlag(string key, bool value)
    {
        if (flags.TryGetValue(key, out var current) && current == value)
            return;

        flags[key] = value;

        _events.Publish(new OnFlagChangedEvent
        {
            Key = key,
            Value = value
        });
    }

    public void SetTrigger(string key, bool value)
    {
        
    }
}