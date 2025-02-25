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

    public static void StartCinematic(Vector2 position)
    {
        var cinematic = Instance.cinematic;

        cinematic = Instance.resourcePreloader.InstanceSceneOrNull<Cinematic>();
        cinematic.CinematicEnded += GameCamera.ClearTargetPositionOverride;

        var scene = GameManager.CurrentScene ?? Instance.GetTree().CurrentScene;
        scene.AddChild(cinematic);

        GameCamera.SetTargetPositionOverride(position);

        Instance.GetTree().CreateTimer(0.1f).Timeout += cinematic.Start;
        Instance.EmitSignal(SignalName.CinematicStarted, position);
    }

    public static void EndCinematic()
    {
        var cinematic = Instance.cinematic;

        if (cinematic == null) return;

        Instance.EmitSignal(SignalName.CinematicEnded);

        cinematic.Stop();
    }
}
