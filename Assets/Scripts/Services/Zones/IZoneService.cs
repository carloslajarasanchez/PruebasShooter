using UnityEngine;

public interface IZoneService
{
    void Initialize();
    void RegisterZone(string id, GameObject root);
    void EnterZone(string id);
    void ExitZone(string id);
}