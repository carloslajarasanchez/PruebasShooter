using UnityEngine;

[System.Serializable]
public class LimbData
{
    public HumanBodyBones bone;
    public float maxHealth = 100f;
    public float instantSeverForce = 20f; // fuerza de un solo golpe que lo arranca
    public bool isCentral = false;        // si muere Die() en el enemigo completo

    [HideInInspector] public float currentHealth;
    [HideInInspector] public bool isSevered;

    public void Initialize()
    {
        currentHealth = maxHealth;
        isSevered = false;
    }
}