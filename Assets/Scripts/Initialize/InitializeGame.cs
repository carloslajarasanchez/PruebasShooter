using System.Collections.Generic;
using UnityEngine;

public class InitializeGame : MonoBehaviour
{
    private IPlayerInput _playerInput;
    private IZoneService _zoneService;
    private ISaveService _saveService;
    private ILogService _logService;
    private IGameState _gameState;
    private IAudioService _audioService;


    private void Awake()
    {
        _playerInput = AppContainer.Get<IPlayerInput>();
        _zoneService = AppContainer.Get<IZoneService>();
        _saveService = AppContainer.Get<ISaveService>();
        _logService = AppContainer.Get<ILogService>();
        _gameState = AppContainer.Get<IGameState>();
        _audioService = AppContainer.Get<IAudioService>();
    }
    private void Start()
    {
        _playerInput.EnablePlayer();
        _zoneService.Initialize();
        _saveService.Load();
        _logService.Add<InitializeGame>($"PersistentDataPath: \n {Application.persistentDataPath}");

        _audioService.PlayBackgroundMusic(SoundType.Asylum);

        if (!_gameState.GetFlag("tutorial_movement"))
        {
            Invoke(nameof(InitWorkflow), 1f);
        }
    }

    private void InitWorkflow()
    {
        var workflowSteps = new List<IStep>()
        {
            new WalkStep(),
            new MoveCameraStep(),
            new CrouchStep(),
        };
        var workflow = new Workflow(workflowSteps);
        workflow.OnComplete += WorkFlowFinished;
        workflow.Begin();
    }

    private void WorkFlowFinished()
    {
        _gameState.SetFlag("tutorial_movement", true);
        _logService.Add<InitializeGame>($"Workflow completo");
    }
}
