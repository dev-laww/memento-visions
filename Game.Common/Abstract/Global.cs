using Godot;

namespace Game.Common.Abstract;

public abstract class Global<T> : Node where T : Global<T>
{
    protected static T Instance { get; private set; } = null!;

    public override void _EnterTree()
    {
        Instance.QueueFree();

        Instance = (T)this;
    }
}