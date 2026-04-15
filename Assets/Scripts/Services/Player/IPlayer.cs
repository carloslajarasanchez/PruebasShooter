using UnityEngine;

public interface IPlayer 
{
    public void RestLives(int amount);
    public void AddLives(int amount);
    public void ResetPlayer();
}
