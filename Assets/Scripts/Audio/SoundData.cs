using UnityEngine;

[CreateAssetMenu(fileName = "SoundData", menuName = "Audio/SoundData")]
public class SoundData : ScriptableObject
{
    public SoundType type;
    public AudioClip[] clips;       // varios clips para variación aleatoria
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    public float pitchVariation = 0f; // rango de variación aleatoria del pitch
    public bool loop = false;
}