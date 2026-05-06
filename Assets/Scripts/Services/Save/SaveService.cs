using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SaveService : ISaveService
{
    private SaveData _data = new();
    private Dictionary<System.Type, object> _states = new();

    private IGameState _gameState;

    private string SavePath => Application.persistentDataPath + "/save.json";

    public SaveService()
    {
        _gameState = AppContainer.Get<IGameState>();
    }

    // ---------------- SAVE ----------------

    public void Save()
    {
        // ---------------- FLAGS / TRIGGERS ----------------

        _data.flags = ToList(_gameState.GetFlags());
        _data.triggers = ToList(_gameState.GetTriggers());

        // ---------------- ITEMS ----------------

        _data.items = new List<ItemSaveEntry>();

        var itemStates = GetStateDictionary<ItemState>();

        foreach (var kvp in itemStates)
        {
            _data.items.Add(new ItemSaveEntry
            {
                id = kvp.Key,
                state = kvp.Value
            });
        }

        // ---------------- DOORS ----------------

        _data.doors = new List<DoorSaveEntry>();

        var doorStates = GetStateDictionary<DoorState>();

        foreach (var kvp in doorStates)
        {
            _data.doors.Add(new DoorSaveEntry
            {
                id = kvp.Key,
                state = kvp.Value
            });
        }

        // ---------------- PLAYER ----------------

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 pos = player.transform.position;

            CameraController cameraCtrl = player.GetComponentInChildren<CameraController>();
            float cameraRotationX = cameraCtrl != null ? cameraCtrl.transform.localEulerAngles.x : 0f;

            _data.player = new PlayerSaveEntry
            {
                x = pos.x,
                y = pos.y,
                z = pos.z,
                playerRotationY = player.transform.eulerAngles.y,
                cameraRotationX = cameraRotationX
            };
        }

        // ---------------- WRITE FILE ----------------

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

        // ---------------- FLAGS / TRIGGERS ----------------

        _gameState.SetFlags(ToDictionary(_data.flags));
        _gameState.SetTriggers(ToDictionary(_data.triggers));

        // ---------------- ITEMS ----------------

        var itemStates = GetStateDictionary<ItemState>();
        itemStates.Clear();

        if (_data.items != null)
        {
            foreach (var item in _data.items)
            {
                itemStates[item.id] = item.state;
            }
        }

        // ---------------- DOORS ----------------

        var doorStates = GetStateDictionary<DoorState>();
        doorStates.Clear();

        if (_data.doors != null)
        {
            foreach (var door in _data.doors)
            {
                doorStates[door.id] = door.state;
            }
        }

        // ---------------- PLAYER ----------------

        if (_data.player != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                PlayerController pc = player.GetComponent<PlayerController>();
                if (pc != null)
                {
                    Vector3 position = new Vector3(_data.player.x, _data.player.y, _data.player.z);
                    pc.RestorePosition(position, _data.player.playerRotationY, _data.player.cameraRotationX);
                }
            }
        }

        RestoreScene();

        Debug.Log("Game Loaded");
    }
    // ---------------- RESTORE SCENE ----------------
    public void RestoreScene()
    {
        // ---------------- ITEMS ----------------

        var items = Object.FindObjectsByType<Item>(FindObjectsSortMode.None);

        foreach (var item in items)
        {
            if (TryGetState<ItemState>(item.SaveId, out var state))
            {
                item.RestoreState(state);
            }
        }

        // ---------------- DOORS ----------------

        var doors = Object.FindObjectsByType<DoorController>(FindObjectsSortMode.None);

        foreach (var door in doors)
        {
            if (TryGetState<DoorState>(door.SaveId, out var state))
            {
                door.RestoreState(state);
            }
        }
    }
    // ---------------- STATE MANAGEMENT ----------------
    // SAVE SIDE (crear si no existe)
    public T GetOrCreateState<T>(string id) where T : new()
    {
        var dict = GetStateDictionary<T>();

        if (!dict.TryGetValue(id, out var state))
        {
            state = new T();
            dict[id] = state;
        }

        return state;
    }

    public void SetState<T>(string id, T state)
    {
        var dict = GetStateDictionary<T>();
        dict[id] = state;
    }

    // LOAD SIDE (solo leer)
    public bool TryGetState<T>(string id, out T state)
    {
        var dict = GetStateDictionary<T>();
        return dict.TryGetValue(id, out state);
    }

    private Dictionary<string, T> GetStateDictionary<T>()
    {
        var type = typeof(T);

        if (!_states.ContainsKey(type))
        {
            _states[type] = new Dictionary<string, T>();
        }

        return (Dictionary<string, T>)_states[type];
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