using Godot;
using System;
using GodotUtilities;

namespace Game.World.Puzzle;

[Scene]
public partial class PressurePlate : Node2D
{
    [Node] Sprite2D sprite;
    [Node] Area2D area;
    [Signal] public delegate void ActivatedEventHandler();
    [Signal] public delegate void DeactivatedEventHandler();

    private int bodies = 0;
    public  bool isActive = false;
    private Rect2 rect;
    
    
    public override void _Ready()
    {
        area.BodyEntered += OnBodyEntered;
        area.BodyExited += OnBodyExited;
        rect = sprite.RegionRect;
    }
    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated || Engine.IsEditorHint()) return;

        WireNodes();
    }
    
    private void OnBodyEntered(Node2D body)
    {
      bodies += 1;
      CheckActive();
    }
    
    private void OnBodyExited(Node2D body)
    {
        bodies -= 1;
        CheckActive();
    }
    
    private void CheckActive()
    {
        if (bodies > 0 && !isActive)
        {
            isActive = true;
            var tempRect = sprite.RegionRect;
            tempRect.Position = new Vector2(rect.Position.X - 32, tempRect.Position.Y);
            sprite.RegionRect = tempRect;
            EmitSignalActivated();
        }
        else if (bodies == 0 && isActive)
        {
            isActive = false;
            sprite.RegionRect = rect;
            EmitSignalDeactivated();
        }
    }
}
