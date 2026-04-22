using System.Collections.Generic;
using UnityEngine;

public class ZoneService : IZoneService
{
    private class ZoneState
    {
        public GameObject root;
        public int refCount;
    }

    private Dictionary<string, ZoneState> zones = new();

    public void Initialize()
    {
        foreach (var zone in zones)
        {
            zone.Value.refCount = 0;
            zone.Value.root.SetActive(false);
        }
    }
    public void RegisterZone(string id, GameObject root)
    {
        if (!zones.ContainsKey(id))
        {
            zones.Add(id, new ZoneState
            {
                root = root,
                refCount = 0
            });
        }
    }

    public void EnterZone(string id)
    {
        if (!zones.ContainsKey(id)) return;

        var zone = zones[id];
        zone.refCount++;

        zone.root.SetActive(true);
    }

    public void ExitZone(string id)
    {
        if (!zones.ContainsKey(id)) return;

        var zone = zones[id];
        zone.refCount = Mathf.Max(0, zone.refCount - 1);

        if (zone.refCount == 0)
            zone.root.SetActive(false);
    }
}