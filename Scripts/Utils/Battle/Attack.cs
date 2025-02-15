using System;
using System.Collections.Generic;
using Game.Components;
using Game.Entities;
using Godot;
using GodotUtilities;

namespace Game.Utils.Battle;

public partial class Attack : RefCounted
{
    public enum Type
    {
        Physical,
        Magical
    }

    public float Damage { get; }

    public Type AttackType { get; }

    public Entity Source { get; }

    public bool Fatal { get; set; }

    public readonly bool Critical;

    public IReadOnlyList<StatusEffect> StatusEffects => statusEffects;

    public bool HasStatusEffects => statusEffects.Count > 0;

    private readonly List<StatusEffect> statusEffects = [];

    private Attack() { }

    private Attack(float damage, Type type, Entity source)
    {
        AttackType = type;
        Critical = MathUtil.RNG.RandfRange(0, 1) < 0.2f;
        Source = source;

        // Apply damage modifiers
        Damage = damage * (Critical ? MathUtil.RNG.RandfRange(1.5f, 2f) : 1);
        Damage *= AttackType == Type.Physical ? 1 : 1.2f;
        Damage = (float)Math.Round(Damage);
    }

    public void AddStatusEffect(StatusEffect effect) => statusEffects.Add(effect);

    public static Attack Create(float damage, Type type, Entity source) => new(damage, type, source);
}