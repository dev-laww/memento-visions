using Godot;
using System;
using Game.Components;
using Game.Entities;
using Game.Data;
using GodotUtilities;

[Scene]
public partial class DesolatePlace : Node2D
{
    [Node] private Entity StoryTeller;
    [Node] private DialogueTrigger DialogueTrigger;
    private Quest quest = ResourceLoader.Load<Quest>("res://resources/quests/Prologue/prologue1.tres");
    public bool isInteracted = false;
    

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        base._Ready();
    }

    public void toggleDialogue()
    {
        DialogueTrigger.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
    }


    public void setStoryTellerVisible()
    {
        StoryTeller.Visible = true;
    }

    public void CompleteObjectiveAtIndex(int index)
    {
        if (quest == null)
        {
            GD.PrintErr("Quest not loaded");
            return;
        }
        
        quest.CompleteObjective(index);
    }
}