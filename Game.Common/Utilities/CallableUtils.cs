using Godot;

namespace Game.Common.Utilities;

public static class CallableUtils
{
    public static Callable FromMethod(Delegate action)
    {
        if (action.Target is not GodotObject target)
        {

            var err = new InvalidOperationException("Action target is not a Godot object.");
            Log.Error(err);

            throw err;
        }

        var methodName = action.Method.Name;

        return new Callable(target, methodName);
    }
}