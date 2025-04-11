using Godot;
using Game.Components;
using GodotUtilities;
using DialogueManagerRuntime;
using Game.Entities;

namespace Game.World;

[Scene]
public partial class PrologueBar : BaseLevel
{
    [Node] private TransitionArea transitionArea;
    [Node] private Player player;
    [Node] private AnimationPlayer animationPlayer;
    [Node] private ScreenMarker screenMarker;
    [Node] private ScreenMarker chiefMarker;

    private bool isInteracted;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        screenMarker.Toggle(false);
        chiefMarker.Toggle(false);
        transitionArea.Toggle(false);
    }

    private static void ShowDialogue()
    {
        var dialouge = ResourceLoader.Load<Resource>("res://resources/dialogues/prologue/player_wonder.dialogue");
        DialogueManager.ShowDialogueBalloon(dialouge);
    }

    private void FinishDialogue()
    {
        chiefMarker.Toggle(false);
        transitionArea.Toggle(true);
        isInteracted = true;
        GetTree().CreateTimer(10f).Timeout += () => screenMarker.Toggle(true);
    }
}