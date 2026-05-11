using UnityEngine;

[RequireComponent(typeof(WorldAudioSource))]
public class DoorController : PusheableObject, ISavable<DoorState>
{
    [SerializeField] private string saveId;

    [Header("Sonido")]
    [SerializeField] private float soundCooldown = 1.5f; // segundos entre reproducciones

    public string SaveId => saveId;

    private ISaveService _saveService;
    private WorldAudioSource _worldAudio;
    private float _lastSoundTime = -Mathf.Infinity; // permite sonar desde el primer push

    private protected override void Awake()
    {
        base.Awake();
        _saveService = AppContainer.Get<ISaveService>();
        _worldAudio = GetComponent<WorldAudioSource>();
        // Seguridad: si no tiene ID, se asigna uno en editor
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(saveId))
            {
                saveId = System.Guid.NewGuid().ToString();
                UnityEditor.EditorUtility.SetDirty(this);
            }
    #endif
    }
    
    public override void Push(Vector3 force)
    {
        base.Push(force);

        if (Time.time - _lastSoundTime >= soundCooldown)
        {
            _worldAudio.Play(SoundType.OpenDoor);
            _lastSoundTime = Time.time;
        }
    }

    public void SaveState()
    {
        var state = _saveService.GetOrCreateState<DoorState>(SaveId);

            state.isOpen = CanBePushed;

        _saveService.SetState(SaveId, state);
    }
    public void RestoreState(DoorState state)
    {
        if (state != null)
        {
            if(state.isOpen)
            {
                base.EnablePushing();
            }
            else if (!state.isOpen)
            {
                base.DisablePushing();
            }

        }
    }
    public override void EnablePushing()
    {
        base.EnablePushing();
        SaveState();
    }

    public override void DisablePushing()
    {
        base.DisablePushing();
        SaveState();
    }
}
