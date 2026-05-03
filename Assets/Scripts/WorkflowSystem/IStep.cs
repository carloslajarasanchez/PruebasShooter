using System;

public interface IStep
{
    public string Name { get; }
    public string Description { get; }
    public bool IsCompleted { get; }
    public void Activate();
    public void Deactivate();
    public event Action OnCompleted;
}
