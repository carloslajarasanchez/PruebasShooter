using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CrouchStep : IStep
{
    public string Name => "Control de agacharse";
    public string Description => "Presiona shift para agacharte, vuelve a presionar shift para levantarte";
    public bool IsCompleted { get => this._isCompleted; set => this._isCompleted = value; }
    public event Action OnCompleted;

    private ILogService _logService;
    private IPlayerInput _playerInput;
    private bool _isCompleted = false;

    public CrouchStep()
    {
        _logService = AppContainer.Get<ILogService>();
        _playerInput = AppContainer.Get<IPlayerInput>();
    }
    public void Activate()
    {
        _logService.Add<WalkStep>($"Activando {this.Name}");
        _logService.Add<WalkStep>($"{this.Description}");
        this._playerInput.Actions.Player.Crouch.performed += HandleAction;
    }

    public void Deactivate()
    {
        this._playerInput.Actions.Player.Crouch.performed -= HandleAction;
    }

    private void HandleAction(InputAction.CallbackContext context)
    {
        this.IsCompleted = true;
        this.OnCompleted?.Invoke();
    }
}
