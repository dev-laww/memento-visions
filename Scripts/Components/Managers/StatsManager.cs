using System;
using System.Collections.Generic;
using Game.Utils.Battle;
using Godot;

namespace Game.Components.Managers;

public enum StatsType
{
    Speed,
    Damage,
    Defense,
    Health,
    Experience,
    Level
}

[Tool]
[GlobalClass, Icon("res://assets/icons/stats-manager.svg")]
public partial class StatsManager : Node
{
    [Signal] public delegate void StatChangedEventHandler(float value, StatsType stat);
    [Signal] public delegate void StatIncreasedEventHandler(float value, StatsType stat);
    [Signal] public delegate void StatDecreasedEventHandler(float value, StatsType stat);
    [Signal] public delegate void StatDepletedEventHandler(StatsType stat);
    [Signal] public delegate void AttackReceivedEventHandler(float dmg, Attack.Type type, bool critical);

    [Export] public float MaxHealth = 100;

    public float Health
    {
        get => health;
        private set => SetStat(ref health, value, StatsType.Health);
    }

    [Export]
    public float Level
    {
        get => level;
        private set => SetStat(ref level, value, StatsType.Level);
    }

    [Export]
    public float Experience
    {
        get => experience;
        private set => SetStat(ref experience, value, StatsType.Experience);
    }

    [Export]
    public float Speed
    {
        get => speed;
        private set => SetStat(ref speed, value, StatsType.Speed);
    }

    [Export]
    public float Defense
    {
        get => defense;
        private set => SetStat(ref defense, value, StatsType.Defense);
    }

    [Export]
    public float Damage
    {
        get => damage;
        private set => SetStat(ref damage, value, StatsType.Damage);
    }

    private float speed = 100;
    private float damage;
    private float health;
    private float experience;
    private float level = 1;
    private float defense;
    private float totalExperience;

    public override void _Ready()
    {
        health = MaxHealth;
    }

    public void Heal(float amount) => Health += amount;
    public void IncreaseLevel(float amount) => Level += amount;

    public void IncreaseExperience(float amount)
    {
        Experience += amount;
        totalExperience += amount;

        while (Experience >= CalculateRequiredExperience(Level + 1))
        {
            Experience -= CalculateRequiredExperience(Level + 1);
            Level++;

            // TODO: Add level up effects
        }
    }

    public void ReceiveAttack(Attack attack)
    {
        Health -= Math.Clamp(attack.Damage - defense, 0, float.MaxValue);

        // TODO: Add status effects
        EmitSignal(SignalName.AttackReceived, attack.Damage, (int)attack.AttackType, attack.Critical);
    }

    private void SetStat(ref float stat, float value, StatsType statType)
    {
        const float TOLERANCE = 0.0001f;
        if (Math.Abs(stat - value) < TOLERANCE) return;

        var oldValue = stat;
        stat = Math.Clamp(value, 0, statType switch
        {
            StatsType.Health => MaxHealth,
            _ => float.MaxValue
        });

        EmitSignal(SignalName.StatChanged, value, (int)statType);
        EmitSignal(value > oldValue ? SignalName.StatIncreased : SignalName.StatDecreased, value, (int)statType);

        if (value <= 0)
            EmitSignal(SignalName.StatDepleted, (int)statType);
    }

    private float CalculateRequiredExperience(float lvl) => (float)(lvl * 4 + Math.Pow(level, 1.8) + 10);

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (Owner is not CharacterBody2D)
            warnings.Add("StatsManager must be  a child of an Entity.");

        return warnings.ToArray();
    }
}