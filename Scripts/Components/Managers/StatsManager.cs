using System;
using System.Collections.Generic;
using System.Linq;
using Game.Common;
using Game.Entities;
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
[GlobalClass, Icon("res://assets/icons/stats-manager.svg")]
public partial class StatsManager : Node
{
    [Signal] public delegate void StatChangedEventHandler(float value, StatsType stat);
    [Signal] public delegate void StatIncreasedEventHandler(float value, StatsType stat);
    [Signal] public delegate void StatDecreasedEventHandler(float value, StatsType stat);
    [Signal] public delegate void StatDepletedEventHandler(StatsType stat);
    [Signal] public delegate void AttackReceivedEventHandler(Attack attack);
    [Signal] public delegate void StatusEffectAddedEventHandler(StatusEffect effect);
    [Signal] public delegate void StatusEffectRemovedEventHandler(StatusEffect effect);

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

    public float SpeedModifier => speedModifiers.Values.Sum();
    public float CalculatedMaxSpeed => speed * (1f + SpeedModifier);
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

    public override void _Ready()
    {
        health = MaxHealth;
        Entity = GetParent() as Entity;

        var effects = GetParent().GetChildren().OfType<StatusEffect>();

        foreach (var effect in effects)
            AddStatusEffect(effect);
    }

    public void Heal(float amount) => Health += amount;
    public void TakeDamage(float amount) => Health -= amount;
    public void IncreaseLevel(float amount) => Level += amount;

    public void IncreaseExperience(float amount)
    {
        Experience += amount;

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
        attack.Fatal = Health <= 0;

        EmitSignalAttackReceived(attack);

        if (!attack.HasStatusEffects) return;

        foreach (var effect in attack.StatusEffects)
            AddStatusEffect(effect);
    }

    public void AddStatusEffect(StatusEffect effect)
    {
        if (!statusEffects.TryAdd(effect.Id, effect)) return;

        effect.Apply();

        if (Entity.GetChildren().OfType<StatusEffect>().All(e => e.Id != effect.Id))
            Entity.AddChild(effect);
        else if (!effect.EditorAdded)
            effect.Stack(effect.StackCount);

        EmitSignalStatusEffectAdded(effect);

        Log.Debug($"{effect} added to {Entity}");
    }

    public void RemoveStatusEffect(string id)
    {
        if (!statusEffects.TryGetValue(id, out var effect)) return;

        effect.Remove();
        effect.QueueFree();

        statusEffects.Remove(id);

        EmitSignalStatusEffectRemoved(effect);

        Log.Debug($"{effect} removed from {Entity}");
    }

    public void Cleanse()
    {
        foreach (var effect in statusEffects.Values)
            RemoveStatusEffect(effect.Id);
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

        EmitSignalStatChanged(value, statType);
        EmitSignal(value > oldValue ? SignalName.StatIncreased : SignalName.StatDecreased, value, (int)statType);

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

    private float CalculateRequiredExperience(float lvl) => (float)(lvl * 4 + Math.Pow(level, 1.8) + 10);

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (GetParent() is not Entities.Entity)
            warnings.Add("StatsManager must be  a child of an Entity.");

        return [.. warnings];
    }
}