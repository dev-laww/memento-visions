using System;
using System.Collections.Generic;
using Game.Components.Battle;
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

    public Node2D Attacker { get; }

    public bool Fatal { get; set; }

    public readonly bool Critical;

    public List<StatusEffect> StatusEffects { get; } = [];

    private Attack() { }

    private Attack(float damage, Type type, Node2D attacker)
    {
        AttackType = type;
        Critical = MathUtil.RNG.RandfRange(0, 1) < 0.2f;
        Attacker = attacker;

        // Apply damage modifiers
        Damage = damage * (Critical ? MathUtil.RNG.RandfRange(1.5f, 2f) : 1);
        Damage *= AttackType == Type.Physical ? 1 : 1.2f;
        Damage = (float)Math.Round(Damage);
    }

    public static Attack Create(float damage, Type type, Node2D attacker) => new(damage, type, attacker);
}