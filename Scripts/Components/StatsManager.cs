using System;
using System.Collections.Generic;
using Godot;
using GodotUtilities;
using StatsResource = Game.Resources.Stats;

namespace Game.Components;

public enum StatsType
{
    Stamina,
    Health,
}

[Tool]
[Scene]
[GlobalClass]
public partial class StatsManager : Node
{
    [Export(PropertyHint.Range, "1,10")]
    private float StaminaRecoveryRate { get; set; } = 1;

    [Export]
    private StatsResource Stats
    {
        get => resource;
        set
        {
            resource = value;
            UpdateConfigurationWarnings();
        }
    }

    [Node]
    private Timer staminaRecovery;

    [Signal]
    public delegate void StatsChangedEventHandler(float value, StatsType stat);

    [Signal]
    public delegate void StatsIncreasedEventHandler(float value, StatsType stat);

    [Signal]
    public delegate void StatsDecreasedEventHandler(float value, StatsType stat);

    [Signal]
    public delegate void StatsDepletedEventHandler(StatsType stat);


    public float Speed { get; private set; }

    public float Health
    {
        get => health;
        private set => health = value;
    }

    public float Stamina
    {
        get => stamina;
        private set => stamina = value;
    }

    public float MaxHealth
    {
        get => maxHealth;
        private set => maxHealth = value;
    }

    private float MaxStamina
    {
        get => maxStamina;
        set => maxStamina = value;
    }

    private float health;
    private float stamina;
    private float maxHealth;
    private float maxStamina;
    private StatsResource resource;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        InitializeStats();

        staminaRecovery.Timeout += () => RecoverStamina(StaminaRecoveryRate);
        staminaRecovery.Start();
    }

    private void InitializeStats()
    {
        Speed = Stats.Speed;
        MaxHealth = Stats.MaxHealth;
        MaxStamina = Stats.MaxStamina;

        // Initialize stats to max values
        Health = MaxHealth;
        Stamina = MaxStamina;
    }

    public void TakeDamage(float amount) => DecreaseStat(
        stat: ref health,
        value: amount,
        type: StatsType.Health
    );

    public void RecoverHealth(float amount) => IncreaseStat(
        stat: ref health,
        max: ref maxHealth,
        value: amount,
        type: StatsType.Health
    );

    public void ConsumeStamina(float amount) => DecreaseStat(
        stat: ref stamina,
        value: amount,
        type: StatsType.Stamina
    );

    public void RecoverStamina(float amount) => IncreaseStat(
        stat: ref stamina,
        max: ref maxStamina,
        value: amount,
        type: StatsType.Stamina
    );

    private void DecreaseStat(ref float stat, float value, StatsType type)
    {
        stat = Math.Max(stat - value, 0);

        var statType = (int)type;
        EmitSignal(SignalName.StatsChanged, Stamina, statType);
        EmitSignal(SignalName.StatsDecreased, value, statType);

        if (stat <= 0)
            EmitSignal(SignalName.StatsDepleted, statType);
    }

    private void IncreaseStat(ref float stat, ref float max, float value, StatsType type)
    {
        if (stat >= max) return;

        stat = Math.Min(stat + value, max);

        var statType = (int)type;
        EmitSignal(SignalName.StatsChanged, Stamina, statType);
        EmitSignal(SignalName.StatsIncreased, value, statType);
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (resource is null)
            warnings.Add("Stats resource is not set.");

        return warnings.ToArray();
    }
}