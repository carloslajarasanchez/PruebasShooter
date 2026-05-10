using System.Collections.Generic;
using UnityEngine;

public class AudioService : MonoBehaviour, IAudioService
{
    private ISoundLibrary _library;
    private List<AudioSource> _pool = new List<AudioSource>();
    private Dictionary<SoundType, AudioSource> _activeSounds = new Dictionary<SoundType, AudioSource>();
    private int _poolSize = 10;

    public void Initialize(ISoundLibrary library)
    {
        _library = library;

        for (int i = 0; i < _poolSize; i++)
        {
            var go = new GameObject($"AudioSource_{i}");
            go.transform.SetParent(transform);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            _pool.Add(source);
        }
    }

    private AudioSource GetFreeSource()
    {
        foreach (var source in _pool)
            if (!source.isPlaying)
                return source;

        // Si no hay libres, expandimos el pool
        var go = new GameObject($"AudioSource_{_pool.Count}");
        go.transform.SetParent(transform);
        var newSource = go.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        _pool.Add(newSource);
        return newSource;
    }

    private void PlayInternal(SoundType type, bool loop, float pitchOverride = -1f)
    {
        var data = _library.Get(type);
        if (data == null || data.clips.Length == 0) return;

        // Si ya está sonando, lo reutilizamos
        if (!_activeSounds.TryGetValue(type, out var source) || source == null)
        {
            source = GetFreeSource();
            _activeSounds[type] = source;
        }

        source.clip = data.clips[Random.Range(0, data.clips.Length)];
        source.volume = data.volume;
        source.pitch = pitchOverride >= 0f ? pitchOverride : data.pitch;
        source.loop = loop;
        source.Play();
    }

    public void Play(SoundType type) => PlayInternal(type, false);

    public void PlayLoop(SoundType type) => PlayInternal(type, true);

    public void PlayWithRandomPitch(SoundType type)
    {
        var data = _library.Get(type);
        if (data == null) return;
        float pitch = data.pitch + Random.Range(-data.pitchVariation, data.pitchVariation);
        PlayInternal(type, false, pitch);
    }

    public void Stop(SoundType type)
    {
        if (_activeSounds.TryGetValue(type, out var source))
        {
            source.Stop();
            _activeSounds.Remove(type);
        }
    }

    public void Pause(SoundType type)
    {
        if (_activeSounds.TryGetValue(type, out var source))
            source.Pause();
    }

    public void Resume(SoundType type)
    {
        if (_activeSounds.TryGetValue(type, out var source))
            source.UnPause();
    }

    public void StopAll()
    {
        foreach (var source in _activeSounds.Values)
            source.Stop();
        _activeSounds.Clear();
    }
}