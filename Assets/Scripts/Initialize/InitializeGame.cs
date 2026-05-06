using System.Collections.Generic;
using UnityEngine;

public class InitializeGame : MonoBehaviour
{
    private IPlayerInput _playerInput;
    private IZoneService _zoneService;
    private ISaveService _saveService;
    private ILogService _logService;

    private void Awake()
    {
        _playerInput = AppContainer.Get<IPlayerInput>();
        _zoneService = AppContainer.Get<IZoneService>();
        _saveService = AppContainer.Get<ISaveService>();
        _logService = AppContainer.Get<ILogService>();
    }
    private void Start()
    {
        _playerInput.EnablePlayer();
        _zoneService.Initialize();
        _saveService.Load();
        _logService.Add<InitializeGame>($"PersistentDataPath: \n {Application.persistentDataPath}");
        InitWorkflow();
    }

    private void InitWorkflow()
    {
        var workflowSteps = new List<IStep>()
        {
            new MoveCameraStep(),
            new WalkStep(),
            new CrouchStep(),
        };
        var workflow = new Workflow(workflowSteps);
        //workflow.OnComplete += WorkFlowFinished;
        workflow.Begin();
    }

    private void WorkFlowFinished()
    {
        _logService.Add<InitializeGame>($"Workflow completo");
    }
}
