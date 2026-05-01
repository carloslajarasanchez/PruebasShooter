using System.Collections.Generic;

public interface IGameState
{
    // FLAGS (estado del mundo)
    bool GetFlag(string key);
    void SetFlag(string key, bool value);

    // TRIGGERS (eventos one-shot)
    bool GetTrigger(string key);
    void SetTrigger(string key, bool value);

    // Para Save/Load
    Dictionary<string, bool> GetFlags();
    Dictionary<string, bool> GetTriggers();

    void SetFlags(Dictionary<string, bool> flags);
    void SetTriggers(Dictionary<string, bool> triggers);
}