using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class WorldAudioSource : MonoBehaviour
{
    private AudioSource _source;
    private ISoundLibrary _library;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _library = AppContainer.Get<ISoundLibrary>();
    }

    public void Play(SoundType type)
    {
        var data = _library.Get(type);
        if (data == null || data.clips.Length == 0) return;

        _source.clip = data.clips[Random.Range(0, data.clips.Length)];
        _source.volume = data.volume;
        _source.pitch = data.pitch;
        _source.loop = data.loop;
        _source.Play();
    }

    public void PlayWithRandomPitch(SoundType type)
    {
        var data = _library.Get(type);
        if (data == null || data.clips.Length == 0) return;

        _source.clip = data.clips[Random.Range(0, data.clips.Length)];
        _source.volume = data.volume;
        _source.pitch = data.pitch + Random.Range(-data.pitchVariation, data.pitchVariation);
        _source.loop = data.loop;
        _source.Play();
    }

    public void Stop() => _source.Stop();
    public void Pause() => _source.Pause();
    public void Resume() => _source.UnPause();
}