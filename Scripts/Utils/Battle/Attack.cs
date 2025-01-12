using System;
using System.Collections.Generic;
using Game.Components.Battle;
using GodotUtilities;

namespace Game.Utils.Battle;

public class Attack
{
    public enum Type
    {
        Physical,
        Magical
    }

    public float Damage { get; }

    public Type AttackType { get; }

    public readonly bool Critical;

    public List<StatusEffect> StatusEffects { get; } = [];

    private Attack(float damage, Type type)
    {
        AttackType = type;
        Critical = MathUtil.RNG.RandfRange(0, 1) < 0.2f;

        // Apply damage modifiers
        Damage = damage * (Critical ? MathUtil.RNG.RandfRange(1.5f, 2f) : 1);
        Damage *= AttackType == Type.Physical ? 1 : 1.2f;
        Damage = (float)Math.Round(Damage);
    }

    public static Attack Create(float damage, Type type) => new(damage, type);
}