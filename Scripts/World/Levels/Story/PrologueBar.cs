using Godot;
using System;
using GodotUtilities;

[Scene]
public partial class PrologueBar : Node2D
{
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
    
    

}
