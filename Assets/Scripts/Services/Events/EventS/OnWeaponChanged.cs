public class OnWeaponChanged : OwnEventBase
{
    public Weapon Weapon { get; set; }
    public int CurrentAmmo { get; set; }
    public int MaxAmmo { get; set; }
}
