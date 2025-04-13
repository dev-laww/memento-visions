using Godot;
using System;
using GodotUtilities;

namespace Game.World;
[Scene]
public partial class CentralForest : BaseLevel
{
    [Node] private Node2D enemy;
    
    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }
    
    public override void _Ready()
    {
        base._Ready();
    }
    public override void _ExitTree()
    {
        base._ExitTree();
        enemy.QueueFree();
    }
}
