using System.Collections.Generic;

public interface IWeaponService
{
    Weapon CurrentWeapon { get; }
    List<Weapon> EquippedWeapons { get; }

    void EquipWeapon(Weapon weapon);
    void UnequipAll();
    void ScrollWeapon(float direction); // +1 siguiente, -1 anterior
}
