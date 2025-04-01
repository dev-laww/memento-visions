using Game.Components;
using Godot;
using GodotUtilities;

namespace Game.UI.Screens;

[Scene]
public partial class Cinematic : CanvasLayer
{
    private const string ANIM_IN = "in";

    [Node] private AnimationPlayer animationPlayer;
    [Node] private ColorRect topBar;
    [Node] private ColorRect bottomBar;

    [Signal] public delegate void CinematicStartedEventHandler();
    [Signal] public delegate void CinematicEndedEventHandler();

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public void Start()
    {
        animationPlayer.Play(ANIM_IN);
        Input.MouseMode = Input.MouseModeEnum.Hidden;
        EmitSignalCinematicStarted();
    }

    public async void Stop()
    {
        animationPlayer.PlayBackwards(ANIM_IN);

        await ToSignal(animationPlayer, "animation_finished");

        EmitSignalCinematicEnded();
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }
}

