using Godot;
using System;
using GodotUtilities;

namespace Game.World.Objects;

[Scene]
public partial class MovableObject : RigidBody2D
{
    public Vector2 pushDirection = Vector2.Zero;
    public Vector2 PushDirection
    {
        get => pushDirection;
        set => SetPush(value);
    }
     float pushSpeed = 7f;
    public bool IsBeingPushed { get; set; } = false;

    public override void _PhysicsProcess(double delta)
    {
     
        if (IsBeingPushed)
        {
            LinearVelocity = pushDirection * pushSpeed;
        }
        else
        {
       
            LinearVelocity = LinearVelocity.Lerp(Vector2.Zero, 0.1f);
        }
    }
    
    private void SetPush(Vector2 value)
    {
        pushDirection = value;
    }
}