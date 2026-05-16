using UnityEngine;

public interface IPlayer 
{
    public int Lives { get; }
    public void RestLives(int amount);
    public void AddLives(int amount);
    public void SetLives(int amount);
    public void ResetPlayer();
}
