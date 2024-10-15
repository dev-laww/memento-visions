using System;
using Godot;
using GodotUtilities;

namespace Game.Components;

public enum Stats
{
    Stamina,
    Health,
}

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
    public delegate void StatsChangedEventHandler(float value, Stats stat);

    [Signal]
    public delegate void StatsIncreasedEventHandler(float value, Stats stat);

    [Signal]
    public delegate void StatsDecreasedEventHandler(float value, Stats stat);

    [Signal]
    public delegate void StatsDepletedEventHandler(Stats stat);

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
        const int stat = (int)Stats.Health;
        EmitSignal(SignalName.StatsChanged, Health, stat);
        EmitSignal(SignalName.StatsDecreased, Health, stat);
        Health = Math.Max(Health - amount, 0);

        if (Health == 0)
            EmitSignal(SignalName.StatsDepleted, stat);
    }

    public void RecoverHealth(float amount)
    {
        if (Math.Abs(Health - MaxHealth) < 0.01) return;

        const int stat = (int)Stats.Health;

        EmitSignal(SignalName.StatsChanged, Health, stat);
        EmitSignal(SignalName.StatsIncreased, Health, stat);

        Health = Math.Min(Health + amount, MaxHealth);
    }

    public void ConsumeStamina(float amount)
    {
        const int stat = (int)Stats.Stamina;

        EmitSignal(SignalName.StatsChanged, Stamina, stat);
        EmitSignal(SignalName.StatsDecreased, amount, stat);
        Stamina = Math.Max(Stamina - amount, 0);

        if (Stamina == 0)
            EmitSignal(SignalName.StatsDepleted, stat);
    }

    public void RecoverStamina(float amount)
    {
        if (Math.Abs(Stamina - MaxStamina) < 0.01) return;

        const int stat = (int)Stats.Stamina;

        EmitSignal(SignalName.StatsChanged, Stamina, stat);
        EmitSignal(SignalName.StatsIncreased, amount, stat);

        Stamina = Math.Min(Stamina + amount, MaxStamina);
    }
}