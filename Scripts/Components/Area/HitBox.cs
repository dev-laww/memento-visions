using System;
using System.Collections.Generic;
using Game.Entities;
using Game.Utils.Battle;
using Godot;

namespace Game.Components;

[Tool]
[GlobalClass, Icon("res://assets/icons/hitbox.svg")]
public partial class HitBox : Area2D
{
    [Export] public Attack.Type Type { get; set; } = Attack.Type.Physical;
    [Export] public float Damage;
    [Export] public float KnockbackForce;

    [Signal] public delegate void HitEventHandler();

    public Entity HitboxOwner;

    public Attack Attack
    {
        get
        {
            if (KnockbackForce <= 0) return Attack.Create(Damage, Type, HitboxOwner ?? Owner as Entity);

            var knockback = new Attack.KnockbackInfo { Direction = Vector2.Zero, Force = KnockbackForce };

            return Attack.Create(Damage, Type, HitboxOwner ?? Owner as Entity, knockback);
        }
    }

    public override void _Ready()
    {
        CollisionLayer = 1 << 10;
        CollisionMask = 1 << 11;
        NotifyPropertyListChanged();
    }

    public void EmitHit() => EmitSignalHit();
}