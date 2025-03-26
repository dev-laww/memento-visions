using Godot;
using System;
using Game.Components;
using Game.Entities;
using GodotUtilities;

[Scene]
public partial class DesolatePlace : Node2D
{
    [Node] private Entity StoryTeller;
    [Node] private DialogueTrigger DialogueTrigger;
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
}
