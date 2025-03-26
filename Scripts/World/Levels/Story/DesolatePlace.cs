using Godot;
using System;
using Game.Components;
using Game.Entities;
using Game.Data;
using GodotUtilities;
namespace Game.Levels.Story;

[Scene]
public partial class DesolatePlace : Node2D
{
    [Node] private Entity StoryTeller;
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