using System;
using UnityEngine.InputSystem;

public class MoveCameraStep : IStep
{
    public string Name => "Controles de la cámara";
    public string Description => "Mueve el ratón para girar la cámara";
    public bool IsCompleted { get => this._isCompleted; set => this._isCompleted = value; }
    public event Action OnCompleted;

    private ILogService _logService;
    private IPlayerInput _playerInput;
    private IAlertService _alertService;
    private bool _isCompleted = false;

    public MoveCameraStep()
    {
        _logService = AppContainer.Get<ILogService>();
        _playerInput = AppContainer.Get<IPlayerInput>();
        _alertService = AppContainer.Get<IAlertService>();
    }

    public void Activate()
    {
        _logService.Add<WalkStep>($"Activando {this.Name}");
        _logService.Add<WalkStep>($"{this.Description}");
        _alertService.Show(this.Description, this.Name);
        this._playerInput.Actions.Player.Camera.performed += HandleAction;
    }

    public void Deactivate()
    {
        this._playerInput.Actions.Player.Move.performed -= HandleAction;
    }

    private void HandleAction(InputAction.CallbackContext context)
    {
        this.IsCompleted = true;
        this.OnCompleted?.Invoke();
    }
}
