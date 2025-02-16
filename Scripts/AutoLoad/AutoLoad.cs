using Godot;

namespace Game.AutoLoad;

public abstract partial class Global<T> : Node where T : Global<T>
{
    protected static T Instance { get; private set; }

    public override void _EnterTree()
    {
        Instance?.QueueFree();

        Instance = (T)this;
    }

    public override void _ExitTree()
    {
        Instance?.QueueFree();
        Instance = null;
    }
}