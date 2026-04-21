/// <summary>
/// Cualquier Item que pueda equiparse en la mano del jugador implementa esta interfaz.
/// Las armas la implementan para disparar, los consumibles para usarse al hacer click.
/// </summary>
public interface IEquippable
{
    /// <summary>Acción principal: click izquierdo. Dispara en armas, usa en consumibles.</summary>
    void OnPrimaryAction();

    /// <summary>El objeto puede seguir usándose tras OnPrimaryAction (armas) o se consume (pociones).</summary>
    bool IsReusable { get; }
}
