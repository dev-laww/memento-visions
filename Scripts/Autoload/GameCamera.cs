using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.Autoload;

[Scene]
public partial class GameCamera : Autoload<GameCamera>
{
    public class ShakeArgs
    {
        public float Force = 20f;
        public float Duration = 0.2f;
        public float Intensity = 100f;
    }

    [Node] private Camera2D shakyCamera2d;

    public static Vector2? TargetPositionOverride { get; private set; }
    private static Vector2 targetPosition;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        Instance.shakyCamera2d.MakeCurrent();
        Callable.From(() => shakyCamera2d.GlobalPosition = this.GetPlayer()?.GlobalPosition ?? Vector2.Zero).CallDeferred();
    }

    public override void _Process(double delta)
    {
        targetPosition = TargetPositionOverride ?? this.GetPlayer()?.GlobalPosition ?? targetPosition;
        shakyCamera2d.GlobalPosition = shakyCamera2d.GlobalPosition.Lerp(targetPosition, 1f - Mathf.Exp(-10f * (float)delta));
    }

    public static void SetTargetPositionOverride(Vector2 position)
    {
        TargetPositionOverride = position;
    }

    public static void ClearTargetPositionOverride()
    {
        TargetPositionOverride = null;
    }

    public static void Shake(ShakeArgs args = null)
    {
        args ??= new ShakeArgs();

        var camera = Instance.shakyCamera2d;

        camera.Set("max_shake_offset", args.Force);
        camera.Set("shake_decay", 1f / args.Duration);
        camera.Call("shake");
    }
}