using System;
using System.Collections.Generic;
using Game.Entities;
using Game.Utils.Battle;
using Godot;
using GodotUtilities.Logic;

namespace Game.Components;

[Tool]
[GlobalClass, Icon("res://assets/icons/hitbox.svg")]
public partial class HitBox : Area2D
{
    [Export] public Attack.Type Type { get; set; } = Attack.Type.Physical;
    [Export] public float Damage;
    [Export] public float KnockbackForce;

    [Signal] public delegate void HitEventHandler();

    public Entity HitBoxOwner;
    private readonly LootTable<string> lootTable = new(); // TODO: make the loot table be able to remove items

    public Attack Attack
    {
        get
        {
            var attack = Attack.Create(Damage, Type, HitBoxOwner ?? Owner as Entity);
            var statusEffect = lootTable.PickItem();

            if (!string.IsNullOrEmpty(statusEffect))
            {
                attack.AddStatusEffect(statusEffect);
            }

            if (KnockbackForce <= 0) return attack;

            attack.AddKnockback(Vector2.Zero, KnockbackForce);

            return attack;
        }
    }

    public override void _Ready()
    {
        CollisionLayer = 1 << 10;
        CollisionMask = 1 << 11;
        NotifyPropertyListChanged();

        lootTable.AddItem("", 10);
    }

    public void AddStatusEffect(string statusEffectId, int weight = 10)
    {
        lootTable.AddItem(statusEffectId, weight);
    }

    public void EmitHit() => EmitSignalHit();
}