using Godot;
using System;
using Game.Components;
using Game.Entities;
using Game.Data;
using GodotUtilities;
namespace Game.Levels.Story;

[Scene]
public partial class SchoolOutdoor : Node2D
{
    [Node] private Entity StoryTeller;
    public int ObjectiveInteracted = 0;
    public bool isStoryTellerVisible = true;
    [Node] TransitionArea TransitionArea;


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
        ((StoryTeller)StoryTeller).Work();
        StoryTeller.Visible = true;
        isStoryTellerVisible = true;
    }


}