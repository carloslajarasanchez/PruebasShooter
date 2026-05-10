using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Audio/SoundLibrary")]
public class SoundLibrary : ScriptableObject, ISoundLibrary
{
    public List<SoundData> sounds;

    private Dictionary<SoundType, SoundData> _map;
    private ILogService _logService;

    public void Initialize()
    {
        _logService = AppContainer.Get<ILogService>();
        _map = new Dictionary<SoundType, SoundData>();
        foreach (var sound in sounds)
        {
            if (!_map.ContainsKey(sound.type))
                _map.Add(sound.type, sound);
        }
    }

    public SoundData Get(SoundType type)
    {
        if (_map != null && _map.TryGetValue(type, out var data))
            return data;

        _logService.Add<SoundLibrary>($"No se encontró el sonido: {type}");
        return null;
    }
}