using System.Collections.Generic;
using UnityEngine;

public class Player : IPlayer
{
    public int Lives { get; private set; } = 100;

    private IEventService _eventService;
    private float _nextDamageTime;
    private const float _damageCooldown = 2f;

    public Player()
    {
        _eventService = AppContainer.Get<IEventService>();
    }

    public void RestLives(int amount)
    {
        if (Time.time < _nextDamageTime) return;
        _nextDamageTime = Time.time + _damageCooldown;

        Lives -= amount;

        if (Lives <= 0)
            _eventService.Publish(new OnGameOver());

        _eventService.Publish(new OnLivesChanged());
    }

    /// <summary>
    /// A�ade vidas al player
    /// </summary>
    /// <param name="amount"></param>
    public void AddLives(int amount)
    {
        Lives += amount;
        _eventService.Publish(new OnLivesChanged());
    }


    public void SetLives(int amount) => Lives = amount;

    public void ResetPlayer()
    {
        Lives = 100;
    }
}
