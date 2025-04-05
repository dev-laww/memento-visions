using Godot;
using System;
using Game.Components;
using GodotUtilities;

namespace Game.World.Levels.Story.Chapter1;

[Scene]
public partial class Village : Node2D
{

    [Node] private TransitionArea TransitionArea;
    [Node] private DialogueTrigger DialogueTrigger;
    [Node] private QuestTrigger QuestTrigger2;
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
