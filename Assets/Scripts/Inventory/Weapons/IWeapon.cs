public interface IWeapon
{
    int CurrentAmmo { get; }
    int MaxAmmo { get; }
    bool IsReloading { get; }

    float FireRate { get; }

    void Shoot();
    void Reload();
}
