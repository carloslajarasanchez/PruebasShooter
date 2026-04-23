using UnityEngine;

public class InitializeGame : MonoBehaviour
{
    private void Start()
    {
        AppContainer.Get<IPlayerInput>().EnablePlayer();
        AppContainer.Get<IZoneService>().Initialize();
    }
}
