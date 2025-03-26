using Game.Data;
using Godot;
using System;
using Game.Data;
using GodotUtilities;

namespace Game.Levels.Story;

[Scene]
public partial class Bar : Node2D
{
    private Quest quest = ResourceLoader.Load<Quest>("res://resources/quests/Prologue/NightofShadows.tres");

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        base._Ready();
    }


    public void CompleteObjectiveAtIndex(int index)
    {
        if (quest == null)
        {
            GD.PrintErr("Quest not loaded");
            return;
        }

        quest.CompleteObjective(1);
    }

}
