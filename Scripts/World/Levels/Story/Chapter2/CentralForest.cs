using Godot;
using System;
using Game.Autoload;
using Game.Entities;
using GodotUtilities;

namespace Game.World;
[Scene]
public partial class CentralForest : BaseLevel
{
    [Node] private Node2D enemy;
    [Node] private StoryTeller storyTeller;
    
    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }
    
    public override void _Ready()
    {
        base._Ready();
    }
    public void StartCinematic()
    {
        CinematicManager.StartCinematic();
        GameCamera.SetTargetPositionOverride(storyTeller.GlobalPosition);
        var timer = GameCamera.Instance.GetTree().CreateTimer(2.5f);
        timer.Timeout += () =>
        {
            GameCamera.SetTargetPositionOverride(Vector2.Zero);
            CinematicManager.EndCinematic();
        };
    }
    public override void _ExitTree()
    {
        base._ExitTree();
        enemy.QueueFree();
    }
}
