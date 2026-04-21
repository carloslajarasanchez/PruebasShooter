using UnityEngine;

public interface IEquipService
{
    Item CurrentItem { get; }
    Item PreviousItem { get; }
    Transform ItemStorage { get; }
    Transform Hand { get; }

    void SetTransforms(Transform itemStorage, Transform hand);
    void Equip(Item item);
    void SwapWithPrevious();
    void Unequip();
    void UseCurrent();
}