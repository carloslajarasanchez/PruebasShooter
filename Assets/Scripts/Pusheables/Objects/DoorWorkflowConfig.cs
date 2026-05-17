using UnityEngine;

[CreateAssetMenu(fileName = "DoorWorkflowConfig", menuName = "Game/Door Workflow Config")]
public class DoorWorkflowConfig : ScriptableObject
{
    public KeyEnum keyType;
    public string stepTypeName; // Nombre de la clase del Step

    public IStep CreateStep()
    {
        // Crear instancia del step dinámicamente
        var stepType = System.Type.GetType(stepTypeName);
        if (stepType != null && typeof(IStep).IsAssignableFrom(stepType))
        {
            return (IStep)System.Activator.CreateInstance(stepType);
        }

        Debug.LogError($"No se pudo crear el step: {stepTypeName}");
        return null;
    }
}