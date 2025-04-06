using Godot;
using System;
using Game.Components;
using Game.Entities;
using Game.Data;
using Game.World.Puzzle;
using GodotUtilities;

namespace Game.World;

[Scene]
public partial class DesolatePlace : Node2D
{
    [Node] private Entity StoryTeller;
    [Node] private TransitionArea TransitionArea;
    [Node] private SmoothTileMapLayer SecretDoor;
    [Node] private PressurePlate PressurePlate;
    [Node] private DialogueTrigger DialogueDoor;
    [Node] private DialogueTrigger Boulder;
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
        TransitionArea.Monitoring = false;
        PressurePlate.Activated += DisableDoor;
        PressurePlate.Deactivated += EnableDoor;
    }

    public void setDialogueDoorOff()
    {
        DialogueDoor.Monitoring = false;
    }

    public void DisableDoor()
    {
        SecretDoor.Enabled = false;
    }

    public void EnableDoor()
    {
        SecretDoor.Enabled = true;
    }

    public void setDialogueBoulderOff()
    {
        Boulder.Monitoring = false;
    }

    public void setStoryTellerVisible()
    {
        GD.Print("setStoryTellerVisible");
        ((StoryTeller)StoryTeller).Work();
        StoryTeller.Visible = true;
        isStoryTellerVisible = true;
        TransitionArea.Monitoring = true;
    }
}