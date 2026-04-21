using UnityEngine;

public class HealthPotion : Item, IEquippable
{
    [SerializeField] private int _healAmount = 20;

    public bool IsReusable => false;

    public void OnPrimaryAction()
    {
        AppContainer.Get<IPlayer>().AddLives(_healAmount);
        Debug.Log($"[HealthPotion] +{_healAmount} vida.");
    }

    public override void Use() => OnPrimaryAction();
}