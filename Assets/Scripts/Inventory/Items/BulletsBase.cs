using UnityEngine;

public abstract class BulletBase : Item, IBullet
{
    public abstract int BulletAmount { get; set; }
    public abstract WeaponTypeEnum Type { get; }

    public override void SaveState(bool? isConsumed = null, int? currentAmmo = null)
    {
        bool consumed = BulletAmount <= 0;
        base.SaveState(isConsumed: consumed, currentAmmo: BulletAmount);
    }

    public override void RestoreState(ItemState state)
    {
        if (state.isConsumed)
        {
            base.RestoreState(state);
            return;
        }
        BulletAmount = state.currentAmmo;
        base.RestoreState(state);
    }
}
