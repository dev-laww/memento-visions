using Game.Components;
using Game.UI.Screens;
using Godot;
using GodotUtilities;

namespace Game.Autoload;

[Scene]
public partial class CinematicManager : Autoload<CinematicManager>
{
    [Node] private ResourcePreloader resourcePreloader;

    [Signal] public delegate void CinematicStartedEventHandler(Vector2 position);
    [Signal] public delegate void CinematicEndedEventHandler();

    private Cinematic cinematic;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public static void StartCinematic(Vector2 position = default)
    {
        Instance.cinematic = Instance.resourcePreloader.InstanceSceneOrNull<Cinematic>();
        Instance.cinematic.CinematicEnded += GameCamera.ClearTargetPositionOverride;

        GameManager.CurrentScene.AddChild(Instance.cinematic);

        GameCamera.SetTargetPositionOverride(position);

        Instance.GetTree().CreateTimer(0.1f).Timeout += Instance.cinematic.Start;
        Instance.EmitSignal(SignalName.CinematicStarted, position);
    }

    public static void EndCinematic()
    {
        var cinematic = Instance.cinematic;

        if (cinematic == null) return;

        cinematic.Stop();

        Instance.EmitSignal(SignalName.CinematicEnded);
        GameCamera.ClearTargetPositionOverride();
    }
}
