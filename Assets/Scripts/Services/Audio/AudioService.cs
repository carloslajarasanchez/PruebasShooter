using System.Collections.Generic;
using UnityEngine;

public class AudioService : MonoBehaviour, IAudioService
{
    private ISoundLibrary _library;
    private List<AudioSource> _pool = new List<AudioSource>();
    private Dictionary<SoundType, AudioSource> _activeSounds = new Dictionary<SoundType, AudioSource>();
    private int _poolSize = 9;
    private GameObject _audioSourceContainer;
    private AudioSource _backgroundMusic;

    public void Initialize(ISoundLibrary library, GameObject go)
    {
        _library = library;

        this._audioSourceContainer = go;
        this.transform.SetParent(transform);

        this._backgroundMusic = this._audioSourceContainer.AddComponent<AudioSource>();

        for (int i = 0; i < _poolSize; i++)
        {
            var source = this._audioSourceContainer.AddComponent<AudioSource>();
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
        var newSource = this._audioSourceContainer.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        _pool.Add(newSource);
        return newSource;
    }

    public void PlayBackgroundMusic(SoundType type)
    {
        if (this._backgroundMusic.isPlaying)
            return;

        var data = _library.Get(type);
        this._backgroundMusic.volume = data.volume;
        this._backgroundMusic.clip = data.clips[0];
        this._backgroundMusic.loop = data.loop;
        this._backgroundMusic.Play();
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

    public bool IsPlaying(SoundType type)
    {
        if (_activeSounds.TryGetValue(type, out var source))
            return source != null && source.isPlaying;
        return false;
    }
}