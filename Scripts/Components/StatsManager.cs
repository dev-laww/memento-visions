using System;
using Godot;
using GodotUtilities;

namespace Game.Components;

[Scene]
[GlobalClass]
public partial class StatsManager : Node
{
    [Export]
    public float MaxHealth { get; set; } = 100f;

    [Export]
    public float Speed { get; set; } = 50f;

    [Export]
    public float MaxStamina { get; set; } = 100f;

    [Export(PropertyHint.Range, "1,10")]
    private float StaminaRecoveryRate { get; set; } = 1;

    [Node]
    private Timer staminaRecovery;

    public float Health { get; private set; }
    public float Stamina { get; private set; }

    [Signal]
    public delegate void HealthChangedEventHandler(float health);

    [Signal]
    public delegate void HealthIncreasedEventHandler(float health);

    [Signal]
    public delegate void HealthDecreasedEventHandler(float health);

    [Signal]
    public delegate void HealthDepletedEventHandler();

    [Signal]
    public delegate void StaminaChangedEventHandler(float stamina);

    [Signal]
    public delegate void StaminaIncreasedEventHandler(float stamina);

    [Signal]
    public delegate void StaminaDecreasedEventHandler(float stamina);

    [Signal]
    public delegate void StaminaDepletedEventHandler();

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        Health = MaxHealth;
        Stamina = MaxStamina;

        staminaRecovery.Timeout += () => RecoverStamina(StaminaRecoveryRate);
        staminaRecovery.Start();
    }

    public void TakeDamage(float amount)
    {
        EmitSignal(SignalName.HealthChanged, Health);
        EmitSignal(SignalName.HealthDecreased, Health);
        Health = Math.Max(Health - amount, 0);

        if (Health == 0)
            EmitSignal(SignalName.HealthDepleted);
    }

    public void RecoverHealth(float amount)
    {
        EmitSignal(SignalName.HealthChanged, Health);
        EmitSignal(SignalName.HealthIncreased, Health);
        Health = Math.Min(Health + amount, MaxHealth);
    }

    public void ConsumeStamina(float amount)
    {
        EmitSignal(SignalName.StaminaChanged, Stamina);
        EmitSignal(SignalName.StaminaDecreased, Stamina);
        Stamina = Math.Max(Stamina - amount, 0);

        if (Stamina == 0)
            EmitSignal(SignalName.StaminaDepleted);
    }

    public void RecoverStamina(float amount)
    {
        if (Stamina + amount > MaxStamina)
        {
            Stamina = MaxStamina;
            return;
        }

        EmitSignal(SignalName.StaminaChanged, Stamina);
        EmitSignal(SignalName.StaminaIncreased, Stamina);
        Stamina = Math.Min(Stamina + amount, MaxStamina);
    }
}