using UnityEngine;

public class PistoleBullets : BulletBase, IBullet
{
    [SerializeField] private int _bulletAmount = 30;

    public override int BulletAmount
    {
        get => _bulletAmount;
        set => _bulletAmount = value;
    }

    public override WeaponTypeEnum Type => WeaponTypeEnum.Pistole;
}
