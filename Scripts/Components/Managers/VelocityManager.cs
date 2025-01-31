using Game.Utils.Extensions;
using Godot;

namespace Game.Components.Managers;

[Tool]
[GlobalClass]
public partial class VelocityManager : Node
{
    [Export] public StatsManager StatsManager;
    [Export] public float AccelerationCoefficient = 10;

    public Vector2 Velocity { get; private set; }
    private float MaxSpeed => StatsManager.Speed;
    private bool signalEmitted;

    [Signal] public delegate void AcceleratingEventHandler();
    [Signal] public delegate void DeceleratingEventHandler();

    public override void _Ready()
    {
        SetProcess(false);
    }

    public void Accelerate(Vector2 direction) => AccelerateToVelocity(direction.TryNormalize() * MaxSpeed);

    public void Decelerate(bool force = false)
    {
        if (signalEmitted)
        {
            EmitSignal(SignalName.Decelerating);
            signalEmitted = true;
        }

        if (force)
        {
            Velocity = Vector2.Zero;
            return;
        }

        AccelerateToVelocity(Vector2.Zero);
    }

    public void AccelerateToVelocity(Vector2 velocity)
    {
        var delta = (float)GetProcessDeltaTime();
        var weight = 1f - Mathf.Exp(-AccelerationCoefficient * delta);
        Velocity = Velocity.Lerp(velocity, weight);
        ApplyMovement();
    }

    private void ApplyMovement()
    {
        if (Owner is not CharacterBody2D owner) return;

        owner.Velocity = Velocity;
        owner.MoveAndSlide();
    }
}