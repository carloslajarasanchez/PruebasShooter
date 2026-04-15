using System.Collections.Generic;

public class Player: IPlayer
{
    public int Lives { get; private set; } = 100;

    private IEventService _eventService;

    public Player()
    {
        _eventService = AppContainer.Get<IEventService>();
    }   
    /// <summary>
    /// Resta vidas al player
    /// </summary>
    /// <param name="amount"></param>
    public void RestLives(int amount)
    {
        Lives -= amount;

        if (Lives <= 0)
        {
            _eventService.Publish(new OnGameOver());
        }

        _eventService.Publish(new OnLivesChanged());
    }

    /// <summary>
    /// Añade vidas al player
    /// </summary>
    /// <param name="amount"></param>
    public void AddLives(int amount)
    {
        Lives += amount;
        _eventService.Publish(new OnLivesChanged());
    }


    public void ResetPlayer()
    {
        Lives = 100;
    }
}
