using Game.Utils.Extensions;
using Game.World.Puzzle;
using Godot;

namespace Game.Scripts.Components.Area;

[Tool]
public partial class PushArea : Area2D
{
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }
    
    private void OnBodyEntered(Node2D body)
    {
        if (body is MovableObject movable)
        {
            movable.IsBeingPushed = true;
            UpdatePushDirection(movable);
        }
    }
    
    private void OnBodyExited(Node2D body)
    {
        if (body is MovableObject movable)
        {
            movable.IsBeingPushed = false;
            movable.pushDirection = Vector2.Zero;
        }
    }
    
    public override void _PhysicsProcess(double delta)
    {
        foreach (var body in GetOverlappingBodies())
        {
            if (body is MovableObject movable && movable.IsBeingPushed)
            {
                UpdatePushDirection(movable);
            }
        }
    }
    
    private void UpdatePushDirection(MovableObject movable)
    {
        var player = this.GetPlayer();
        if (player != null)
        {
            var direction = player.GetPlayerDirection();
            movable.pushDirection = direction.LengthSquared() > 0 ? direction : Vector2.Zero;
        } 
    }
    // nahihila
    

}