using System.Collections.Generic;

public class Player: IPlayer
{
    public int Lives { get; private set; } = 3;

    public List<Item> Inventory { get; private set; } = new List<Item>();


    /// <summary>
    /// Resta vidas al player
    /// </summary>
    /// <param name="amount"></param>
    public void RestLives(int amount)
    {
        Lives -= amount;

        if (Lives <= 0)
        {
            AppContainer.Get<IEventService>().Publish(GameEvents.OnGameOver);
        }

        AppContainer.Get<IEventService>().Publish(GameEvents.OnLivesChanged);
    }

    /// <summary>
    /// Añade vidas al player
    /// </summary>
    /// <param name="amount"></param>
    public void AddLives(int amount)
    {
        Lives += amount;
        AppContainer.Get<IEventService>().Publish(GameEvents.OnLivesChanged);
    }


    public void ResetPlayer()
    {
        Lives = 100;
    }

    public void AddItem(Item item)
    {
        Inventory.Add(item);
        AppContainer.Get<IEventService>().Publish(GameEvents.OnInventoryChanged);
    }

    public void RemoveItem(Item item) {
        Inventory.Remove(item);
        AppContainer.Get<IEventService>().Publish(GameEvents.OnInventoryChanged);
    }
}
