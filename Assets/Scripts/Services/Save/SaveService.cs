using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SaveService : ISaveService
{
    private SaveData _data = new();
    private Dictionary<string, ItemState> _itemStates = new();

    private IGameState _gameState;

    private string SavePath => Application.persistentDataPath + "/save.json";

    public SaveService()
    {
        _gameState = AppContainer.Get<IGameState>();
    }

    // ---------------- SAVE ----------------

    public void Save()
    {
        // Flags / Triggers to List
        _data.flags = ToList(_gameState.GetFlags());
        _data.triggers = ToList(_gameState.GetTriggers());

        // Items to List
        _data.items = new List<ItemSaveEntry>();

        foreach (var kvp in _itemStates)
        {
            _data.items.Add(new ItemSaveEntry
            {
                id = kvp.Key,
                state = kvp.Value
            });
        }

        string json = JsonUtility.ToJson(_data, true);
        File.WriteAllText(SavePath, json);

        Debug.Log("Game Saved");
    }

    // ---------------- LOAD ----------------

    public void Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("No save file found");
            return;
        }

        string json = File.ReadAllText(SavePath);
        _data = JsonUtility.FromJson<SaveData>(json);


        var flags = ToDictionary(_data.flags);
        var triggers = ToDictionary(_data.triggers);

        _gameState.SetFlags(flags);
        _gameState.SetTriggers(triggers);

        // Items
        _itemStates = new Dictionary<string, ItemState>();

        if (_data.items != null)
        {
            foreach (var item in _data.items)
            {
                _itemStates[item.id] = item.state;
            }
        }
        RestoreScene();
        Debug.Log("Game Loaded");
    }
    // ---------------- RESTORE SCENE ----------------
    public void RestoreScene()
    {
        var items = Object.FindObjectsByType<Item>(FindObjectsSortMode.None);

        foreach (var item in items)
        {
            if (_itemStates.TryGetValue(item.SaveId, out var state))
            {
                item.RestoreState(state);
            }
        }
    }

    // ---------------- ITEM STATE ----------------

    public ItemState GetItemState(string id)
    {
        if (!_itemStates.TryGetValue(id, out var state))
        {
            state = new ItemState();
            _itemStates[id] = state;
        }

        return state;
    }

    public void SetItemState(string id, ItemState state)
    {
        _itemStates[id] = state;
    }

    // ---------------- HELPERS ----------------

    private List<FlagEntry> ToList(Dictionary<string, bool> dict)
    {
        var list = new List<FlagEntry>();

        foreach (var kvp in dict)
        {
            list.Add(new FlagEntry
            {
                key = kvp.Key,
                value = kvp.Value
            });
        }

        return list;
    }

    private Dictionary<string, bool> ToDictionary(List<FlagEntry> list)
    {
        var dict = new Dictionary<string, bool>();

        if (list == null) return dict;

        foreach (var entry in list)
        {
            dict[entry.key] = entry.value;
        }

        return dict;
    }
}