using UnityEngine;

public class OnEnemyKilled : OwnEventBase
{
    public GameObject Enemy { get; set; }
    public float Points { get; set; }
    public bool PowerUpDropped { get; set; }
}
