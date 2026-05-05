using UnityEngine;

public class ShotgunBullets : BulletBase
{
    [SerializeField] private int _bulletAmount = 20;

    public override int BulletAmount
    {
        get => _bulletAmount;
        set => _bulletAmount = value;
    }

    public override WeaponTypeEnum Type => WeaponTypeEnum.Shotgun;
}