using UnityEngine;

public class InitializerMusicScene : MonoBehaviour
{
    private IAudioService _audioService; // Referencia al servicio de audio para iniciar la música de fondo.

    private void Awake()
    {
        _audioService = AppContainer.Get<IAudioService>(); // Obtenemos el servicio de audio del contenedor de servicios para poder usarlo.
    }

    private void Start()
    {
        _audioService.PlayBackgroundMusic(SoundType.MusicMain);
            
    }
}
