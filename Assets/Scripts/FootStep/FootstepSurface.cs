using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FootstepSurface", menuName = "Audio/FootstepSurface")]
public class FootstepSurface : ScriptableObject
{
    [Serializable]
    public class SurfaceEntry
    {
        public PhysicsMaterial material;
        public SoundType soundType;
    }

    public List<SurfaceEntry> surfaces;
    public SoundType defaultSound = SoundType.FootstepConcrete; // fallback

    public SoundType GetSoundForMaterial(PhysicsMaterial material)
    {
        if (material == null) return defaultSound;

        foreach (var entry in surfaces)
            if (entry.material == material)
                return entry.soundType;

        return defaultSound;
    }
}