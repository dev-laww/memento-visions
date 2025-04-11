using System;
using System.Collections.Generic;
using Game.Common;
using Game.Utils.Extensions;
using Godot;
using Godot.Collections;

namespace Game.Components;


[Tool]
[GlobalClass, Icon("res://assets/icons/velocity_manager.svg")]
public partial class VelocityManager : Node
{
    [Export]
    private StatsManager StatsManager
    {
        get => statsManager;
        set
        {
            statsManager = value;
            UpdateConfigurationWarnings();
        }
    }

    [Export] public float AccelerationCoefficient = 10f;

    [ExportCategory("Dash")]
    [Export(PropertyHint.Range, "0,5,1")]
    public int TimesCanDash = 1;

    [Export] public bool CanDashWhileDashing;
    [Export] public float DashSpeed = 200;
    [Export] public float DashDuration = 0.3f;
    [Export] public float DashCoolDown = 4f;

    [Signal] public delegate void DashedEventHandler(Vector2 position);
    [Signal] public delegate void DashFreedEventHandler(Vector2 position);
    [Signal] public delegate void TeleportedEventHandler(Vector2 origin, Vector2 destination);

    public Vector2 Velocity { get; private set; }
    public Vector2 LastFacedDirection { get; private set; } = Vector2.Down;

    public bool IsDashing
    {
        get => isDashing;
        private set
        {
            if (isDashing == value) return;

            isDashing = value;

            if (value) EmitSignalDashed(Body.GlobalPosition);
        }
    }

    private CharacterBody2D Body => GetParent() as CharacterBody2D;
    private Array<Vector2> dashQueue = [];
    private bool isDashing;
    private bool knockbacked;
    private StatsManager statsManager;

    public enum FacingDirectionMode
    {
        FourDirections,
        EightDirections
    }

    /// <summary>
    /// Called when the node is added to the scene. Disables processing until movement is explicitly applied.
    /// </summary>
    public override void _Ready()
    {
        SetProcess(false);
        SetPhysicsProcess(false);
    }

    /// <summary>
    /// Accelerates the entity in the specified direction.
    /// </summary>
    /// <param name="direction">The direction vector to accelerate towards. This vector is normalized internally.</param>
    /// <returns>
    /// Returns the current instance (<see cref="VelocityManager"/>) to allow method chaining.
    /// </returns>
    /// <remarks>
    /// The acceleration is calculated using the current <see cref="CalculatedMaxSpeed"/>, which factors in base speed and any temporary modifiers.
    /// </remarks>
    public VelocityManager Accelerate(Vector2 direction)
    {
        return AccelerateToVelocity(direction.TryNormalize() * StatsManager.CalculatedMaxSpeed);
    }

    /// <summary>
    /// Decelerates the entity.
    /// </summary>
    /// <param name="force">
    /// If set to <c>true</c>, the entity's velocity is immediately set to zero.
    /// Otherwise, the entity gradually decelerates to zero using the current acceleration parameters.
    /// </param>
    /// <returns>
    /// Returns the current instance (<see cref="VelocityManager"/>) to allow method chaining.
    /// </returns>
    public VelocityManager Decelerate(bool force = false)
    {
        if (!force)
            return AccelerateToVelocity(Vector2.Zero);

        Velocity = Vector2.Zero;
        return this;
    }

    /// <summary>
    /// Smoothly accelerates the entity toward a target velocity.
    /// </summary>
    /// <param name="velocity">The target velocity to approach.</param>
    /// <param name="accelerationCoefficient">
    /// The acceleration coefficient to use. If set to zero, the default <see cref="AccelerationCoefficient"/> is used.
    /// </param>
    /// <returns>
    /// Returns the current instance (<see cref="VelocityManager"/>) to allow method chaining.
    /// </returns>
    /// <remarks>
    /// The method uses an exponential decay formula to calculate the interpolation weight based on the elapsed time.
    /// </remarks>
    public VelocityManager AccelerateToVelocity(Vector2 velocity, float accelerationCoefficient = 0)
    {
        accelerationCoefficient = accelerationCoefficient == 0 ? AccelerationCoefficient : accelerationCoefficient;
        var delta = (float)GetProcessDeltaTime();
        var weight = 1f - Mathf.Exp(-accelerationCoefficient * delta);

        Velocity = Velocity.Lerp(velocity, weight);

        if (!Velocity.IsZeroApprox() && !knockbacked)
            LastFacedDirection = Velocity.TryNormalize();

        return this;
    }

    /// <summary>
    /// Initiates a dash action in the specified direction.
    /// </summary>
    /// <param name="direction">
    /// The direction vector in which to dash.
    /// If not provided (or default), the last faced direction (<see cref="LastFacedDirection"/>) is used.
    /// </param>
    /// <returns>
    /// Returns the current instance (<see cref="VelocityManager"/>) to allow method chaining.
    /// </returns>
    /// <remarks>
    /// This method checks if the dash can be executed using <see cref="CanDash(Vector2)"/>.
    /// If the dash is allowed, it sets the velocity based on <see cref="DashSpeed"/> and a multiplier determined by the current dash queue length.
    /// Timers are then created to handle the dash duration and cooldown, emitting corresponding signals when each period ends.
    /// </remarks>
    public VelocityManager Dash(Vector2 direction = default)
    {
        if (!CanDash(direction) || IsDashing)
            return this;

        if (direction == default)
            direction = LastFacedDirection;

        LastFacedDirection = direction;
        IsDashing = true;

        var multiplier = 1f + dashQueue.Count * 0.1f;
        Velocity = direction.TryNormalize() * DashSpeed * multiplier * (1f + StatsManager.SpeedModifier);

        dashQueue.Add(Body.GlobalPosition);

        GetTree().CreateTimer(DashDuration).Timeout += OnDashDurationTimeout;
        GetTree().CreateTimer(DashCoolDown).Timeout += OnDashCooldownTimeout;

        return this;
    }

    /// <summary>
    /// Applies a knockback force to the entity in the specified direction.
    /// </summary>
    /// <param name="direction">The direction in which to apply the knockback force. The direction is normalized internally.</param>
    /// <param name="force">The magnitude of the knockback force.</param>
    /// <returns>
    /// Returns the current instance (<see cref="VelocityManager"/>) to allow method chaining.
    /// </returns>
    /// <remarks>
    /// Knockback immediately sets the entity's velocity based on the provided direction and force.
    /// </remarks>
    public VelocityManager Knockback(Vector2 direction, float force)
    {
        Log.Debug($"{Body} knocked back to {direction} with force {force}");
        Velocity = direction.TryNormalize() * force;

        knockbacked = true;

        GetTree().CreateTimer(0.5f).Timeout += () =>
        {
            Decelerate(true);
            knockbacked = false;
        };

        return this;
    }

    /// <summary>
    /// Teleports the entity to a new position.
    /// </summary>
    /// <param name="destination">The target position to which the entity should be teleported.</param>
    /// <returns>
    /// Returns the current instance (<see cref="VelocityManager"/>) to allow method chaining.
    /// </returns>
    /// <remarks>
    /// This method emits the <see cref="TeleportedEventHandler"/> signal with both the original and destination positions,
    /// and then updates the entity's global position.
    /// </remarks>
    public VelocityManager Teleport(Vector2 destination)
    {
        Log.Debug($"{Body} teleported from {Body.GlobalPosition} to {destination}");
        EmitSignalTeleported(Body.GlobalPosition, destination);
        Body.GlobalPosition = destination;
        return this;
    }

    /// <summary>
    /// Moves the entity's body based on the current velocity and the provided time delta.
    /// </summary>
    /// <param name="delta">
    /// The time interval over which to move the entity.
    /// If set to zero, the current process delta time is used.
    /// </param>
    /// <returns>
    /// Returns a <see cref="KinematicCollision2D"/> object containing collision details if a collision occurs during movement;
    /// otherwise, returns <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This method updates the <see cref="CharacterBody2D"/>'s velocity and calls <see cref="CharacterBody2D.MoveAndCollide(Vector2)"/>
    /// to handle collision detection. It is designed to be frame rate independent.
    /// </remarks>
    public KinematicCollision2D MoveAndCollide(float delta = 0f)
    {
        if (delta == 0f)
            delta = (float)GetProcessDeltaTime();

        Body.Velocity = Velocity;

        return Body.MoveAndCollide(Velocity * delta);
    }

    /// <summary>
    /// Overrides the entity's last faced direction.
    /// </summary>
    /// <param name="direction">The new direction to assign as the last faced direction.</param>
    /// <returns>
    /// Returns the current instance (<see cref="VelocityManager"/>) to allow method chaining.
    /// </returns>
    /// <remarks>
    /// This method is useful for updating the directional state of the entity independently from its movement.
    /// </remarks>
    public VelocityManager OverrideLastFacedDirection(Vector2 direction)
    {
        LastFacedDirection = direction;
        return this;
    }

    /// <summary>
    /// Determines whether the entity can perform a dash in the specified direction.
    /// </summary>
    /// <param name="direction">
    /// The direction vector to check. A non-zero vector is required to perform a dash.
    /// If not provided, the method will consider the default value.
    /// </param>
    /// <returns>
    /// Returns <c>true</c> if the entity is allowed to dash (i.e., the dash count has not been exceeded, a valid direction is provided,
    /// and the entity is not already dashing unless <see cref="CanDashWhileDashing"/> is enabled); otherwise, returns <c>false</c>.
    /// </returns>
    public bool CanDash(Vector2 direction = default)
    {
        var available = TimesCanDash > dashQueue.Count && !direction.IsZeroApprox();
        var whileDashing = CanDashWhileDashing || (!CanDashWhileDashing && !IsDashing);
        return available && whileDashing;
    }

    /// <summary>
    /// Determines the last faced direction of the entity as a string.
    /// </summary>
    /// <param name="mode">
    /// The facing direction mode to use. The default is <see cref="FacingDirectionMode.FourDirections"/>.
    /// </param>
    /// <returns>
    /// Returns a string representing the last faced direction of the entity.
    /// </returns>
    public string GetLastFacedDirectionString(FacingDirectionMode mode = FacingDirectionMode.FourDirections)
    {
        return mode switch
        {
            FacingDirectionMode.FourDirections => GetFourDirectionString(),
            FacingDirectionMode.EightDirections => GetEightDirectionString(),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }


    /// <summary>
    /// Gets the last faced direction of the entity as a string using four cardinal directions.
    /// </summary>
    /// <returns>
    /// Returns a string representing the last faced direction of the entity.
    /// </returns>
    public string GetFourDirectionString()
    {
        var lastMoveDirection = LastFacedDirection;

        if (lastMoveDirection == Vector2.Zero) return "down";

        var x = lastMoveDirection.X;
        var y = lastMoveDirection.Y;

        if (Math.Abs(x) > Math.Abs(y))
        {
            if (x > 0)
                return "right";

            return "left";
        }

        if (y < 0)
            return "up";

        return "down";
    }

    /// <summary>
    /// Gets the last faced direction of the entity as a string using eight cardinal directions.
    /// </summary>
    /// <returns>
    /// Returns a string representing the last faced direction of the entity.
    /// </returns>
    public string GetEightDirectionString()
    {
        var lastMoveDirection = LastFacedDirection;

        if (lastMoveDirection == Vector2.Zero) return "down";

        var x = Mathf.Sign(lastMoveDirection.X);
        var y = Mathf.Sign(lastMoveDirection.Y);

        if (x == 0 && y == 0) return "down";

        if (x == 0)
            return y > 0 ? "up" : "down";

        if (y == 0)
            return x > 0 ? "right" : "left";

        if (x > 0)
            return y > 0 ? "right-up" : "right-down";

        return y > 0 ? "left-up" : "left-down";
    }


    public void ApplyMovement()
    {
        if (Owner is not CharacterBody2D owner) return;

        owner.Velocity = Velocity;
        owner.MoveAndSlide();
    }

    private void OnDashCooldownTimeout()
    {
        Log.Debug($"{Body} cooldown ended");
        if (dashQueue.Count == 0) return;

        var direction = dashQueue[^1];
        dashQueue.RemoveAt(dashQueue.Count - 1);
        EmitSignalDashFreed(direction);
    }

    private void OnDashDurationTimeout()
    {
        IsDashing = false;
        var lastDash = dashQueue.Count > 0 ? dashQueue[^1] : Vector2.Zero;
        EmitSignalDashFreed(lastDash);
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (StatsManager == null)
            warnings.Add("StatsManager is not set.");

        if (GetParent() is not CharacterBody2D)
            warnings.Add("VelocityManager should be a child of a CharacterBody2D node.");

        return [.. warnings];
    }
}