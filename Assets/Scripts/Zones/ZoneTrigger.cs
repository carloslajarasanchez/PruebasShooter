using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    public string zoneId;

    private IZoneService zoneService;

    private void Awake()
    {
        zoneService = AppContainer.Get<IZoneService>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            zoneService.EnterZone(zoneId);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            zoneService.ExitZone(zoneId);
    }
}