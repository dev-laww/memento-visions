using System;
using System.Collections.Generic;
using System.Linq;
using Game.Common;
using Game.Entities;
using Game.Data;
using Game.Utils.Battle;
using Godot;

namespace Game.Components;

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
[GlobalClass, Icon("res://assets/icons/stats_manager.svg")]
public partial class StatsManager : Node
{
    [Signal] public delegate void StatChangedEventHandler(float value, StatsType stat);
    [Signal] public delegate void StatIncreasedEventHandler(float value, StatsType stat);
    [Signal] public delegate void StatDecreasedEventHandler(float value, StatsType stat);
    [Signal] public delegate void StatDepletedEventHandler(StatsType stat);
    [Signal] public delegate void AttackReceivedEventHandler(Attack attack);
    [Signal] public delegate void StatusEffectAddedEventHandler(StatusEffect effect);
    [Signal] public delegate void StatusEffectRemovedEventHandler(StatusEffect effect);
    [Signal] public delegate void DamageTakenEventHandler(float damage);
    [Signal] public delegate void LevelUpEventHandler(float level);

    [Export] private bool Invulnerable;

    [Export]
    public float Level
    {
        get => level;
        private set
        {
            SetStat(ref level, value, StatsType.Level);
            ApplyLevelToStats();
        }
    }

    [Export] public float MaxHealth = 100;

    public float Health
    {
        get => health;
        private set => SetStat(ref health, value, StatsType.Health);
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

    public float SpeedModifier => speedModifiers.Values.Sum();
    public float CalculatedMaxSpeed => speed * (1f + SpeedModifier);
    public Attack LastReceivedAttack { get; private set; }
    public IReadOnlyDictionary<string, StatusEffect> StatusEffects => statusEffects;

    private float speed = 100;
    private float damage;
    private float health;
    private float experience;
    private float level = 1;
    private float defense;
    private readonly Dictionary<string, float> speedModifiers = [];
    private readonly Dictionary<string, StatusEffect> statusEffects = [];
    private Entity Entity;
    private float baseMaxHealth;
    private float baseDamage;
    private float baseDefense;

    public override void _Ready()
    {
        health = MaxHealth;
        Entity = GetParent() as Entity;

        baseMaxHealth = MaxHealth;
        baseDamage = Damage;
        baseDefense = Defense;

        ApplyLevelToStats();
    }

    // Health
    public void Heal(float amount, ModifyMode mode = ModifyMode.Value)
    {
        Health += mode switch
        {
            ModifyMode.Percentage => MaxHealth * (amount / 100f),
            ModifyMode.Value => amount,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

    public void TakeDamage(float amount, ModifyMode mode = ModifyMode.Value)
    {
        Health -= mode switch
        {
            ModifyMode.Percentage => MaxHealth * (amount / 100f),
            ModifyMode.Value => amount,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };

        EmitSignalDamageTaken(amount);
    }

    // Damage
    public void IncreaseDamage(float amount, ModifyMode mode = ModifyMode.Value)
    {
        Damage += mode switch
        {
            ModifyMode.Percentage => Damage * (amount / 100f),
            ModifyMode.Value => amount,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

    public void DecreaseDamage(float amount, ModifyMode mode = ModifyMode.Value)
    {
        Damage -= mode switch
        {
            ModifyMode.Percentage => Damage * (amount / 100f),
            ModifyMode.Value => amount,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

    // Defense
    public void IncreaseDefense(float amount, ModifyMode mode = ModifyMode.Value)
    {
        Defense += mode switch
        {
            ModifyMode.Percentage => Damage * (amount / 100f),
            ModifyMode.Value => amount,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

    public void DecreaseDefense(float amount, ModifyMode mode = ModifyMode.Value)
    {
        Defense -= mode switch
        {
            ModifyMode.Percentage => Damage * (amount / 100f),
            ModifyMode.Value => amount,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

    public void IncreaseLevel(float amount)
    {
        Level += amount;
        ApplyLevelToStats();
        EmitSignalLevelUp(Level);
    }

    public void IncreaseExperience(float amount)
    {
        Experience += amount;

        var requiredExperience = CalculateRequiredExperience(Level + 1);
        var levelsIncreased = 0;

        while (Experience >= requiredExperience)
        {
            Experience -= requiredExperience;
            levelsIncreased++;
            requiredExperience = CalculateRequiredExperience(Level + levelsIncreased + 1);
        }

        if (levelsIncreased > 0) IncreaseLevel(levelsIncreased);
    }

    public void SetLevel(float value)
    {
        Level = value;
        ApplyLevelToStats();
    }

    public void SetExperience(float value)
    {
        experience = value;
    }

    private void ApplyLevelToStats()
    {
        var healthRatio = Health / MaxHealth;

        MaxHealth = baseMaxHealth + CalculateStatLevelDelta(StatsType.Health, Level);
        damage = baseDamage + CalculateStatLevelDelta(StatsType.Damage, Level);
        defense = baseDefense + CalculateStatLevelDelta(StatsType.Defense, Level);
        health = MaxHealth * healthRatio;
    }

    private static float CalculateStatLevelDelta(StatsType stat, float level)
    {
        return stat switch
        {
            StatsType.Health => 20f * (level - 1) + Mathf.Pow(level, 1.8f) - Mathf.Pow(1, 1.8f),
            StatsType.Damage => 2f * (level - 1) + Mathf.Pow(level, 1.5f) - Mathf.Pow(1, 1.5f),
            StatsType.Defense => 1.5f * (level - 1) + Mathf.Pow(level, 1.3f) - Mathf.Pow(1, 1.3f),
            _ => throw new ArgumentOutOfRangeException(nameof(stat), stat, null)
        };
    }

    public void ReceiveAttack(Attack attack)
    {
        if (!Invulnerable)
            Health -= Math.Clamp(attack.Damage - defense, 0, float.MaxValue);

        LastReceivedAttack = attack;
        EmitSignalAttackReceived(attack);

        if (!attack.HasStatusEffects) return;

        foreach (var effect in attack.StatusEffects)
            AddStatusEffect(effect);
    }

    public void AddStatusEffect(StatusEffect effect)
    {
        if (!statusEffects.TryGetValue(effect.Id, out var existing))
        {
            effect.ApplyStatusEffect(Entity);
            statusEffects.Add(effect.Id, effect);
            Log.Debug($"{effect} added to {Entity}");
            EmitSignalStatusEffectAdded(effect);
        }
        else if (existing.StackCount < existing.MaxStacks)
        {
            existing.Stack(effect.StackCount);
            Log.Debug($"Added {effect.StackCount} stacks to {existing}");
        }
    }

    public void RemoveStatusEffect(string id)
    {
        if (!statusEffects.TryGetValue(id, out var effect)) return;

        effect.Remove();

        statusEffects.Remove(id);

        EmitSignalStatusEffectRemoved(effect);

        Log.Debug($"{effect} removed from {Entity}");
    }

    public void Cleanse()
    {
        foreach (var effect in statusEffects.Values)
            RemoveStatusEffect(effect.Id);
    }

    public void SetInvulnerable(bool value)
    {
        Invulnerable = value;
    }

    public void ApplySpeedModifier(string id, float modifier)
    {
        if (speedModifiers.TryGetValue(id, out var value))
            speedModifiers[id] = value + modifier;
        else
            speedModifiers.Add(id, modifier);

        EmitSignalStatChanged(CalculatedMaxSpeed, StatsType.Speed);
    }

    public void RemoveSpeedModifier(string id, float modifier = 0)
    {
        if (!speedModifiers.TryGetValue(id, out var value)) return;

        value = modifier > 0 ? value - modifier : value + Math.Abs(modifier);

        if (value == 0 || modifier == 0)
            speedModifiers.Remove(id);
        else
            speedModifiers[id] = value;

        EmitSignalStatChanged(CalculatedMaxSpeed, StatsType.Speed);
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

        var diff = Math.Abs(value - oldValue);

        EmitSignalStatChanged(value, statType);
        EmitSignal(value > oldValue ? SignalName.StatIncreased : SignalName.StatDecreased, diff, (int)statType);

        if (value <= 0)
            EmitSignalStatDepleted(statType);
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach (var effect in statusEffects.Values)
        {
            effect.Update();

            if (effect.RemainingDuration > 0) continue;

            RemoveStatusEffect(effect.Id);
        }
    }

    public static float CalculateRequiredExperience(float lvl) => (float)(lvl * 4 + Math.Pow(lvl, 1.8) + 10);
    public static float CalculateExperienceReward(float lvl) => (float)(lvl * 2 + Math.Pow(lvl, 1.2));

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (GetParent() is not Entities.Entity)
            warnings.Add("StatsManager must be  a child of an Entity.");

        return [.. warnings];
    }

    public enum ModifyMode
    {
        Percentage,
        Value
    }
}