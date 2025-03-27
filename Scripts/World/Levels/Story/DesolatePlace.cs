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
    public bool isInteracted = false;
    public int ObjectiveInteracted = 0;
    public bool isStoryTellerVisible = false;
    

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
        isStoryTellerVisible = true;
    }
    
}