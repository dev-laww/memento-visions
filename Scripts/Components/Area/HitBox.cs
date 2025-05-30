﻿using System.Collections.Generic;
using Game.Entities;
using Game.Data;
using Game.Utils;
using Game.Utils.Battle;
using Godot;

namespace Game.Components;

// TODO: fix hitbox not creating the same attack when hitting multiple hurtboxes at once, (this includes refactoring on tikbalang and aswang enemies)
[Tool]
[GlobalClass, Icon("res://assets/icons/hitbox.svg")]
public partial class HitBox : Area2D
{
    [Export] public Attack.Type Type { get; set; } = Attack.Type.Physical;
    [Export] public float Damage;
    [Export] public float KnockbackForce;

    [Signal] public delegate void HitEventHandler();

    public Entity HitBoxOwner;
    private readonly List<StatusEffect.Info> statusEffects = [];
    private Attack attackOverride;

    public Attack Attack => GetAttack();

    public override void _Ready()
    {
        CollisionLayer = 1 << 10;
        CollisionMask = 1 << 11;
        NotifyPropertyListChanged();
    }

    public void SetAttackOverride(Attack attack) => attackOverride = attack;
    public void ClearAttackOverride() => attackOverride = null;

    // TODO: refactor to be applied on damage factory
    public void AddStatusEffectToPool(string statusEffectId, int turns = 1)
    {
        var statusEffect = new StatusEffect.Info
        {
            Id = statusEffectId,
            IsGuaranteed = true,
            Chance = 1,
            Turns = turns,
        };

        statusEffects.Add(statusEffect);
    }

    public void AddStatusEffectToPool(string statusEffectId, float chance, int turns = 1)
    {
        var statusEffect = new StatusEffect.Info
        {
            Id = statusEffectId,
            IsGuaranteed = false,
            Chance = chance,
            Turns = turns,
        };

        statusEffects.Add(statusEffect);
    }

    public void AddStatusEffectToPool(StatusEffect.Info statusEffect)
    {
        if (statusEffect.IsGuaranteed)
        {
            statusEffect.Chance = 1;
        }

        statusEffects.Add(statusEffect);
    }

    public void ClearStatusEffectPool() => statusEffects.Clear();

    private Attack GetAttack()
    {
        if (attackOverride != null) return attackOverride;

        var attack = new DamageFactory.AttackBuilder(HitBoxOwner ?? Owner as Entity)
            .SetDamage(Damage)
            .SetType(Type)
            .SetKnockback(KnockbackForce)
            .SetStatusEffectPool(statusEffects)
            .Build();

        var effects = statusEffects.ToArray();

        foreach (var effect in effects)
        {
            if (--effect.Turns > 0) continue;

            statusEffects.Remove(effect);
        }

        return attack;
    }

    public void EmitHit() => EmitSignalHit();
}