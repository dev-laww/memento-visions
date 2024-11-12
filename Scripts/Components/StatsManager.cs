using System;
using System.Collections.Generic;
using Game.Battle;
using Godot;
using GodotUtilities;
using StatsResource = Game.Resources.Stats;

namespace Game.Components;

// TODO: add methods for modifying other stats
// TODO: Buff and debuff system maybe??

public enum StatsType
{
    Mana,
    Speed,
    Attack,
    Defense,
    Magic,
    Health,
}

[Tool]
[Scene]
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

    [Signal]
    public delegate void AttackReceivedEventHandler(float damage, Attack.Type type, bool isCritical = false);

    public float Speed { get; private set; }

    public float Health
    {
        get => health;
        private set => health = value;
    }

    public float Magic
    {
        get => magic;
        private set => magic = value;
    }

    public float Defense
    {
        get => defense;
        private set => defense = value;
    }

    public float Attack
    {
        get => attack;
        private set => attack = value;
    }

    public float Mana
    {
        get => mana;
        private set => mana = value;
    }

    public float MaxHealth
    {
        get => maxHealth;
        private set => maxHealth = value;
    }

    private float MaxMana
    {
        get => maxMana;
        set => maxMana = value;
    }

    public Attack MagicalAttack => Battle.Attack.Magical(Attack);
    public Attack PhysicalAttack => Battle.Attack.Physical(Attack);
    public float PhysicalDamageMultiplier => Stats.PhysicalDamageMultiplier;
    public float MagicalDamageMultiplier => Stats.MagicalDamageMultiplier;
    private float health;
    private float mana;
    private float maxHealth;
    private float maxMana;
    private float magic;
    private float defense;
    private float attack;
    private StatsResource resource;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        InitializeStats();

        staminaRecovery.Timeout += () => RecoverMana(StaminaRecoveryRate);
        staminaRecovery.Start();
    }

    private void InitializeStats()
    {
        if (Stats is null) return;

        Speed = Stats.Speed;
        MaxHealth = Stats.MaxHealth;
        MaxMana = Stats.MaxMana;
        Attack = Stats.Attack;
        Magic = Stats.Magic;
        Defense = Stats.Defense;

        Health = MaxHealth;
        Mana = MaxMana;
    }

    public void ReceiveAttack(Attack atk)
    {
        EmitSignal(SignalName.AttackReceived, atk.Damage, (int)atk.AttackType, atk.IsCritical);
        DecreaseStat(
            stat: ref health,
            value: atk.Damage,
            type: StatsType.Health
        );
    }

    public void RecoverHealth(float amount) => IncreaseStat(
        stat: ref health,
        max: ref maxHealth,
        value: amount,
        type: StatsType.Health
    );

    public void ConsumeMana(float amount) => DecreaseStat(
        stat: ref mana,
        value: amount,
        type: StatsType.Mana
    );

    public void RecoverMana(float amount) => IncreaseStat(
        stat: ref mana,
        max: ref maxMana,
        value: amount,
        type: StatsType.Mana
    );

    private void DecreaseStat(ref float stat, float value, StatsType type)
    {
        stat = Math.Max(stat - value, 0);

        var statType = (int)type;
        EmitSignal(SignalName.StatsChanged, stat, statType);
        EmitSignal(SignalName.StatsDecreased, value, statType);

        if (stat <= 0)
            EmitSignal(SignalName.StatsDepleted, statType);
    }

    private void IncreaseStat(ref float stat, ref float max, float value, StatsType type)
    {
        if (stat >= max) return;

        stat = Math.Min(stat + value, max);

        var statType = (int)type;
        EmitSignal(SignalName.StatsChanged, stat, statType);
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