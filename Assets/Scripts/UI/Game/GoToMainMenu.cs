using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToMainMenu : MonoBehaviour
{
    public void OnGoToMainMenuClicked()
    {
        Time.timeScale = 1f;
        AppContainer.Get<IEventService>().Clear();
        AppContainer.Get<IEquipService>().Clear();
        AppContainer.Get<IInventoryService>().Clear();
        AppContainer.Get<IGameState>().Clear();
        AppContainer.Get<IZoneService>().Clear();
        AppContainer.Get<ISaveService>().ClearStates();
        AppContainer.Get<IPlayerInput>().SwitchControlMap(ControlMap.UI);
        var pauseService = AppContainer.Get<IPauseService>();
        if (pauseService != null)
        {
            pauseService.IsPauseBlocked = false;
            pauseService.Resume();
        }
        SceneManager.LoadScene("02_MainMenu");
    }
}
