using System;
using System.Collections.Generic;
using System.Linq;
using Game.Common;
using Game.Utils.Extensions;
using Godot;
using Godot.Collections;
using GodotUtilities;

namespace Game.Components.Managers;

internal class TemporarySpeed
{
    public float Speed;
    public float Duration;
}

[Tool]
[Scene]
[GlobalClass]
public partial class VelocityManager : Node
{
    [Export] private StatsManager StatsManager;
    [Export] private float AccelerationCoefficient = 10;

    [ExportCategory("Dash")]
    [Export(PropertyHint.Range, "0,5,1")]
    private int TimesCanDash = 1;

    [Export]
    private float DashDuration
    {
        get => (float?)dashDurationTimer?.WaitTime ?? 0;
        set
        {
            if (dashDurationTimer == null) return;

            dashDurationTimer.WaitTime = value;
            dashDurationTimer.NotifyPropertyListChanged();
        }
    }

    [Export]
    private float DashCoolDown
    {
        get => (float?)dashCooldownTimer?.WaitTime ?? 0;
        set
        {
            if (dashCooldownTimer == null) return;

            dashCooldownTimer.WaitTime = value;
            dashCooldownTimer.NotifyPropertyListChanged();
        }
    }

    [Export] private float DashSpeed = 200;
    [Export] private float DashAccelerationCoefficient = 150;

    [Node] private Timer dashDurationTimer;
    [Node] private Timer dashCooldownTimer;

    [Signal] public delegate void DashedEventHandler(Vector2 position);
    [Signal] public delegate void DashFreedEventHandler(Vector2 position);
    [Signal] public delegate void TeleportedEventHandler(Vector2 origin, Vector2 destination);

    public Vector2 Velocity { get; private set; }
    public CharacterBody2D Body => Owner as CharacterBody2D;
    public Vector2 LastFacedDirection { get; private set; } = Vector2.Down;
    public bool CanDash => dashQueue.Count < TimesCanDash;

    public bool IsDashing
    {
        get => isDashing;
        set
        {
            if (isDashing == value) return;

            isDashing = value;

            if (value) EmitSignal(SignalName.Dashed, Body.GlobalPosition);
        }
    }

    private Array<Vector2> dashQueue = [];
    private List<TemporarySpeed> temporarySpeeds = [];
    private bool isDashing;

    private float CalculatedMaxSpeed => Math.Max(
        0,
        temporarySpeeds.Aggregate(0f, (acc, x) => acc + x.Speed) + StatsManager.Speed
    );


    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        SetProcess(false);
    }

    public VelocityManager Accelerate(Vector2 direction)
    {
        LastFacedDirection = direction;
        return AccelerateToVelocity(direction.TryNormalize() * CalculatedMaxSpeed);
    }

    public VelocityManager Decelerate(bool force = false)
    {
        if (!force) return AccelerateToVelocity(Vector2.Zero);

        Velocity = Vector2.Zero;
        return this;
    }

    public VelocityManager AccelerateToVelocity(Vector2 velocity, float accelerationCoefficient = 0)
    {
        accelerationCoefficient = accelerationCoefficient == 0 ? AccelerationCoefficient : accelerationCoefficient;
        var delta = (float)GetProcessDeltaTime();
        var weight = 1f - Mathf.Exp(-accelerationCoefficient * delta);
        Velocity = Velocity.Lerp(velocity, weight);

        return this;
    }

    public VelocityManager Dash(Vector2 direction = default)
    {
        if (direction == default) direction = LastFacedDirection;
        if (!CanDash) return this;
        LastFacedDirection = direction;
        IsDashing = true;
        AccelerateToVelocity(direction.TryNormalize() * DashSpeed, DashAccelerationCoefficient);

        dashQueue.Add(Body.GlobalPosition);
        dashDurationTimer.Start();
        dashCooldownTimer.Start();

        return this;
    }

    public VelocityManager Knockback(Vector2 direction, float force)
    {
        Log.Debug($"{Body} knocked back to {direction} with force {force}");
        Velocity = direction.TryNormalize() * force;

        return this;
    }

    public VelocityManager Teleport(Vector2 destination)
    {
        Log.Debug($"{Body} teleported from {Body.GlobalPosition} to {destination}");
        EmitSignal(SignalName.Teleported, Body.GlobalPosition, destination);
        Body.GlobalPosition = destination;
        return this;
    }

    public VelocityManager ApplyTemporarySpeed(float speed, float duration = 0)
    {
        var temporarySpeed = new TemporarySpeed { Speed = speed, Duration = duration };

        temporarySpeeds.Add(temporarySpeed);
        Log.Debug($"Temporary speed {speed} applied for {duration} seconds");
        Log.Debug($"Current speed: {CalculatedMaxSpeed}");

        GetTree().CreateTimer(duration).Timeout += () =>
        {
            temporarySpeeds.Remove(temporarySpeed);
            Log.Debug($"Temporary speed {speed} removed after {duration} seconds");
            Log.Debug($"Current speed: {CalculatedMaxSpeed}");
        };

        return this;
    }

    public VelocityManager OverrideLastFacedDirection(Vector2 direction)
    {
        LastFacedDirection = direction;
        return this;
    }

    public void ApplyMovement()
    {
        if (Owner is not CharacterBody2D owner) return;

        owner.Velocity = Velocity;
        owner.MoveAndSlide();
    }

    private void OnDashCooldownTimeout()
    {
        Log.Debug("Dash cooldown ended");
        if (dashQueue.Count == 0) return;

        var direction = dashQueue[^1];
        dashQueue.RemoveAt(dashQueue.Count - 1);
        EmitSignal(SignalName.DashFreed, direction);
    }

    private void OnDashDurationTimeout()
    {
        IsDashing = false;
        var lastDash = dashQueue.Count > 0 ? dashQueue[^1] : Vector2.Zero;
        EmitSignal(SignalName.Dashed, lastDash);
    }
}