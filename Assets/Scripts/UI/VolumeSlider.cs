using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour
{
    private Slider _slider;
    private IAudioService _audioService;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
        _audioService = AppContainer.Get<IAudioService>();
        _slider.value = _audioService.MasterVolume;
        _slider.onValueChanged.AddListener(OnVolumeChanged);
    }

    private void OnVolumeChanged(float value)
    {
        _audioService.MasterVolume = value;
    }
}
