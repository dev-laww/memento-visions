using Godot;
using System;
using Game.Components;
using Game.World.Puzzle;
using GodotUtilities;

namespace Game.World;

[Scene]
public partial class Village : BaseLevel
{
    [Node] private TransitionArea TransitionArea;
    [Node] private DialogueTrigger DialogueTrigger;
    [Node] private QuestTrigger QuestTrigger2;
    [Node] private Chest Chest , Chest2;
    [Node] private LeverManager LeverManager;
    [Node]  private TorchPuzzleManager LightPuzzle;

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated && !Engine.IsEditorHint())
            WireNodes();
    }

    public override void _Ready()
    {
        base._Ready();
        QuestTrigger2.Monitoring = false;
        TransitionArea.Monitoring = false;
        LeverManager.IsComplete += OnLeverPuzzleComplete;
        LightPuzzle.PuzzleSolved += OnLightPuzzleComplete;
    }

    private void OnLeverPuzzleComplete()
    {
        Chest.Visible = true;
    }
    private void OnLightPuzzleComplete()
    {
        Chest2.Visible = true;
    }

    public void SetDialogueTriigerOff()
    {
        DialogueTrigger.Monitoring = false;
    }

    public void setQuestTriggerOn()
    {
        QuestTrigger2.Monitoring = true;
    }

    public void setTransitionAreaOn()
    {
        TransitionArea.Monitoring = true;
    }
}