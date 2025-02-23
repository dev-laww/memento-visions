using DialogueManagerRuntime;
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

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        DialogueManager.ShowDialogueBalloon(GD.Load<Resource>("res://resources/dialogues/Test.dialogue"), key: "Start");
    }

    public void Start()
    {
        animationPlayer.Play(ANIM_IN);
    }

    public async void Stop()
    {
        animationPlayer.PlayBackwards(ANIM_IN);

        await ToSignal(animationPlayer, "animation_finished");

        // QueueFree();
    }
}

