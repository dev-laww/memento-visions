using System.Collections.Generic;
using Game.Utils.Extensions;
using Godot;


namespace Game.Components.Movement;

[Tool]
[GlobalClass]
public partial class Velocity : Node
{
    [Export]
    private StatsManager Stats
    {
        get => resource;
        set
        {
            resource = value;
            UpdateConfigurationWarnings();
        }
    }

    [Export]
    private float Acceleration { get; set; } = 100f;

    [Export]
    private float Deceleration { get; set; } = 6f;

    [Signal]
    public delegate void DeceleratingEventHandler();

    [Signal]
    public delegate void AcceleratingEventHandler();

    private Vector2 velocity
    {
        get => v;
        set => v = value.SnapToGrid();
    }

    public bool IsOwnerMoving => !velocity.IsZeroApprox();
    private Node2D Parent => GetParent() as Node2D;

    private StatsManager resource;
    private Vector2 v = Vector2.Zero;
    private bool accelerating;

    public void Accelerate(Vector2 direction)
    {
        if (direction.IsZeroApprox())
        {
            Decelerate();
            return;
        }

        if (!accelerating)
        {
            accelerating = true;
            EmitSignal(SignalName.Accelerating);
        }

        velocity = velocity.MoveToward(direction.Normalized() * Stats.Speed, Acceleration);

        ApplyMovement();
    }

    public void Decelerate()
    {
        if (accelerating)
        {
            accelerating = false;
            EmitSignal(SignalName.Decelerating);
        }

        velocity = velocity.MoveToward(Vector2.Zero, Deceleration);

        ApplyMovement();
    }

    private void ApplyMovement()
    {
        if (Parent is CharacterBody2D)
        {
            var character = GetParent<CharacterBody2D>();

            character.Velocity = velocity;
            character.MoveAndSlide();
            return;
        }

        var body = GetParent<RigidBody2D>();
        body.LinearVelocity = velocity;
    }

    public void Teleport(Vector2 targetPosition) => Parent.GlobalPosition = targetPosition;

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (Parent is not (CharacterBody2D or RigidBody2D))
        {
            warnings.Add("Velocity component should be attached to a CharacterBody2D or RigidBody2D node.");
        }

        if (Stats == null)
        {
            warnings.Add("StatsManager is not set.");
        }

        return warnings.ToArray();
    }
}