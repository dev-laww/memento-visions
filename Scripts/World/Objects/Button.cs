using Godot;
using System;
using GodotUtilities;

namespace Game.World.Objects;
[Scene]
public partial class Button : Sprite2D
{
    [Node] private Area2D Area2D;

    
    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated || Engine.IsEditorHint()) return;

        WireNodes();
    }
    
    public override void _Ready()
    {
        base._Ready();
        Area2D.BodyEntered += OnBodyEntered;
        Area2D.BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node body)
    {
        Frame = 0;
      Owner.Call("PushInput", GetIndex());
    }
    private void OnBodyExited(Node body)
    {
        Frame = 1;
    }
}
