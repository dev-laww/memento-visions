using Godot;

namespace Game.Common.Utilities;

public static class CallableUtils
{
    public static Callable FromMethod(Delegate action)
    {
        if (action.Target is GodotObject) return Callable.From((Action)action);

        var err = new InvalidOperationException("Action target is not a Godot object.");
        Log.Error(err);

        throw err;
    }
}