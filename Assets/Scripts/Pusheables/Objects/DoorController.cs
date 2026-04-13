using UnityEngine;

public class DoorController : PusheableObject
{
    public override void Push(Vector3 force)
    {
        base.Push(force);
        // Reproducir sonido de puerta al ser empujada
    }
}
