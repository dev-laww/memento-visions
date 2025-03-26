using Godot;
using System;
using GodotUtilities;

namespace Game.Levels.Story;

[Scene]
public partial class TownSquare : Node2D
{
    
    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }
    public override void _Ready()
    {
        base._Ready();
        
    }
    
}
