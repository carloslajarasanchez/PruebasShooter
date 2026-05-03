using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class WalkStep : IStep
{
    public string Name => "Controles de movimiento";
    public string Description => "Presiona WASD para moverte por el espacio";
    public bool IsCompleted { get => this._isCompleted; set => this._isCompleted = value; }
    public event Action OnCompleted;

    private ILogService _logService;
    private IPlayerInput _playerInput;
    private bool _isCompleted = false;

    public WalkStep()
    {
        _logService = AppContainer.Get<ILogService>();
        _playerInput = AppContainer.Get<IPlayerInput>();
    }

    public void Activate()
    {
        _logService.Add<WalkStep>($"Activando {this.Name}");
        _logService.Add<WalkStep>($"{this.Description}");
        this._playerInput.Actions.Player.Move.performed += HandleAction;
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
