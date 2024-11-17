using System.Collections.Generic;
using Game.Components.Managers;
using Game.Extensions;
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

    [Export]
    private float DashDuration { get; set; } = 0.1f;

    [Signal]
    public delegate void DeceleratingEventHandler();

    [Signal]
    public delegate void AcceleratingEventHandler();

    [Signal]
    public delegate void DashStartedEventHandler();

    [Signal]
    public delegate void DashEndedEventHandler();

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
    private bool dashing;
    private Timer dashTimer;

    public override void _Ready()
    {
        // Setup dash duration timer
        dashTimer = new Timer
        {
            WaitTime = DashDuration,
            OneShot = true
        };
        AddChild(dashTimer);
        dashTimer.Timeout += EndDash;
    }

    public void Accelerate(Vector2 direction)
    {
        if (dashing) return;

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
        if (dashing) return;

        if (accelerating)
        {
            accelerating = false;
            EmitSignal(SignalName.Decelerating);
        }

        velocity = velocity.IndependentMoveToward(Vector2.Zero, Deceleration);

        ApplyMovement();
    }

    public void Stop()
    {
        velocity = Vector2.Zero;
        ApplyMovement();
    }

    public void Dash(Vector2 direction, float multiplier = 2.25f)
    {
        if (direction.IsZeroApprox()) return;

        if (!dashing)
        {
            dashing = true;
            dashTimer.Start();
            EmitSignal(SignalName.DashStarted);
        }

        velocity = direction * (Stats.Speed * multiplier);
        ApplyMovement();
    }

    private void EndDash()
    {
        dashing = false;
        velocity = Vector2.Zero;
        EmitSignal(SignalName.DashEnded);
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