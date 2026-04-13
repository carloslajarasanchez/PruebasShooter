using static PlayerInputManager;

public interface IPlayerInput
{
    PlayerInputActions Actions { get; }
    public void SwitchControlMap(ControlMap map);
    public void EnablePlayer();
    public void EnableUI();
    public void DisableAll();
}
