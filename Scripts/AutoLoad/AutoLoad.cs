using Game.Common;
using Godot;

namespace Game.AutoLoad;

public abstract partial class AutoLoad<T> : Node where T : AutoLoad<T>
{
    public static T Instance { get; private set; }

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

    public static void ConnectToSignal(StringName signal, Callable target, uint flags = 0)
    {
        if (Instance == null)
        {
            Log.Warn($"{typeof(T).Name} is not loaded.");
            return;
        }

        if (Instance.IsConnected(signal, target))
        {
            Log.Warn($"{typeof(T).Name} is already connected to {signal}.");
            return;
        }

        Instance.Connect(signal, target, flags);
    }

    public static void DisconnectFromSignal(StringName signal, Callable target)
    {
        if (Instance == null)
        {
            Log.Warn($"{typeof(T).Name} is not loaded.");
            return;
        }

        if (!Instance.IsConnected(signal, target))
        {
            Log.Warn($"{typeof(T).Name} is not connected to {signal}.");
            return;
        }

        Instance.Disconnect(signal, target);
    }
}