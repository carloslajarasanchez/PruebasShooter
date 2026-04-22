using UnityEngine;

public class Zone : MonoBehaviour
{
    public string zoneId;

    private void Awake()
    {
        AppContainer.Get<IZoneService>().RegisterZone(zoneId, gameObject);
    }
}