using UnityEngine;

public class InitializeGame : MonoBehaviour
{
    private void Awake()
    {
        AppContainer.Get<IPlayerInput>().EnablePlayer();
        AppContainer.Get<IZoneService>().Initialize();
    }
}
