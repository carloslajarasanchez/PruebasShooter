using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WorldAudioSource))]
public class DoorController : PusheableObject, ISavable<DoorState>
{
    [SerializeField] private string saveId;

    [Header("Sonido")]
    [SerializeField] private float soundCooldown = 1.5f; // segundos entre reproducciones
    [SerializeField] private KeyEnum _requiredKeyType; // Tipo de llave requerida para abrir la puerta

    [Header("Workflow")]
    [SerializeField] private DoorWorkflowConfig _workflowConfig; // Asigna en el inspector

    public string SaveId => saveId;

    private ISaveService _saveService;
    private IInventoryService _inventoryService;
    private ILogService _logService;
    private WorldAudioSource _worldAudio;
    private float _lastSoundTime = -Mathf.Infinity; // permite sonar desde el primer push

    private protected override void Awake()
    {
        base.Awake();
        _saveService = AppContainer.Get<ISaveService>();
        _worldAudio = GetComponent<WorldAudioSource>();
        _inventoryService = AppContainer.Get<InventoryService>();
        _logService = AppContainer.Get<ILogService>();
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

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        CheckKey();  
    }

    private void CheckKey()
    {
        foreach (Item item in _inventoryService.Items)
        {
            if (item is Key key && key.GetTypeKey() == _requiredKeyType)
            {
                this.CanBePushed = true;
                return; // Salir si encuentra la llave
            }
        }

        // No tiene la llave necesaria - solo mostrar mensaje si la puerta está cerrada
        if (!CanBePushed)
        {
            InitWorkflow();
        }
    }

    private void InitWorkflow()
    {
        if (_workflowConfig == null)
        {
            _logService.Add<DoorController>("No hay configuración de workflow para esta puerta");
            return;
        }

        var step = _workflowConfig.CreateStep();
        if (step != null)
        {
            var workflowSteps = new List<IStep>() { step };
            var workflow = new Workflow(workflowSteps);
            workflow.Begin();
        }
    }
}
