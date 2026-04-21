/// <summary>
/// Servicio central de equipamiento. Reemplaza a IWeaponService.
/// Gestiona qué objeto está en la mano y el historial de los 2 últimos equipados.
/// </summary>
public interface IEquipService
{
    Item CurrentItem { get; }
    Item PreviousItem { get; }

    void Equip(Item item);
    void SwapWithPrevious();
    void Unequip();
    void UseCurrent();
}
