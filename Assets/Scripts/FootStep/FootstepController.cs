using UnityEngine;

public class FootstepController : MonoBehaviour
{
    [Header("Detecci�n de suelo")]
    public float raycastDistance = 1.2f;
    public LayerMask groundLayer;

    [Header("Configuraci�n")]
    public FootstepSurface footstepSurface;

    [Header("Umbrales del bob")]
    [Tooltip("Valor de Sin por debajo del cual se considera que el pie toca el suelo")]
    public float stepThreshold = -0.7f;

    private IAudioService _audioService;
    private IPlayerInput _playerInput;
    private IEventService _eventService;

    private bool _stepReady = true; // evita que suene m�s de una vez por ciclo
    private bool _isCrouching;

    private void Awake()
    {
        _audioService = AppContainer.Get<IAudioService>();
        _playerInput = AppContainer.Get<IPlayerInput>();
        _eventService = AppContainer.Get<IEventService>();
    }

    private void OnEnable()
    {
        _eventService.Subscribe<OnPlayerCrouch>(OnCrouch);
        _eventService.Subscribe<OnPlayerStand>(OnStand);
    }

    private void OnDisable()
    {
        _eventService.Unsubscribe<OnPlayerCrouch>(OnCrouch);
        _eventService.Unsubscribe<OnPlayerStand>(OnStand);
    }

    private void OnCrouch(OwnEventBase e) => _isCrouching = true;
    private void OnStand(OwnEventBase e) => _isCrouching = false;

    // Llamado desde CameraController al actualizar el step bob
    public void OnBobUpdate(float bobSinValue, bool isMoving)
    {
        if (!isMoving)
        {
            _stepReady = true;
            return;
        }

        // Cuando el seno baja del umbral = pie en el suelo
        if (bobSinValue < stepThreshold && _stepReady)
        {
            _stepReady = false;
            PlayFootstep();
        }
        else if (bobSinValue > 0f)
        {
            // Pie en el aire, preparamos el siguiente paso
            _stepReady = true;
        }
    }

    private void PlayFootstep()
    {
        PhysicsMaterial material = GetGroundMaterial();
        SoundType sound = footstepSurface.GetSoundForMaterial(material);
        _audioService.PlayWithRandomPitch(sound);
    }

    private PhysicsMaterial GetGroundMaterial()
    {
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, groundLayer))
            return hit.collider.sharedMaterial;

        return null;
    }
}