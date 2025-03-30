using System;
using System.Collections.Generic;
using Game.Common;
using Game.Components;
using Game.Data;
using Game.Entities;
using Game.Utils.Battle;
using Godot;
using GodotUtilities;


namespace Game.Utils;

public static class DamageFactory
{
    private static readonly PackedScene LineDamageScene = ResourceLoader.Load<PackedScene>("res://Scenes/Components/Battle/Damage/LineDamage.tscn");

    private static readonly PackedScene CircleDamageScene = ResourceLoader.Load<PackedScene>("res://Scenes/Components/Battle/Damage/CircleDamage.tscn");

    public class LineDamageBuilder(Vector2 start, Vector2 end)
    {
        private float duration = -1;
        private Entity owner;
        private Attack.Type type;
        private float damage = 1;

        public LineDamageBuilder SetOwner(Entity owner)
        {
            this.owner = owner;
            return this;
        }

        public LineDamageBuilder SetDuration(float duration)
        {
            this.duration = duration;
            return this;
        }

        public LineDamageBuilder SetDamage(float damage)
        {
            this.damage = damage;
            return this;
        }

        public LineDamageBuilder SetDamageType(Attack.Type type)
        {
            this.type = type;
            return this;
        }

        public LineDamage Build()
        {
            var instance = LineDamageScene.Instantiate<LineDamage>();

            instance.HitBox.Type = type;
            instance.HitBox.Damage = damage;
            instance.HitBox.HitBoxOwner = owner;

            GameManager.CurrentScene.AddChild(instance);

            instance.Start(start, end, duration);
            return instance;
        }
    }

    public class HitBoxBuilder(Vector2 position)
    {
        private float duration = -1;
        private Entity owner;
        private Attack.Type type = Attack.Type.Physical;
        private float damage = 1;
        private float rotation;
        private Vector2 shapeOffset;
        private Shape2D shape;

        public HitBoxBuilder SetOwner(Entity owner)
        {
            this.owner = owner;
            return this;
        }

        public HitBoxBuilder SetDuration(float duration)
        {
            this.duration = duration;
            return this;
        }

        public HitBoxBuilder SetRotation(float rotation)
        {
            this.rotation = rotation;
            return this;
        }

        public HitBoxBuilder SetDamage(float damage)
        {
            this.damage = damage;
            return this;
        }

        public HitBoxBuilder SetDamageType(Attack.Type type)
        {
            this.type = type;
            return this;
        }

        public HitBoxBuilder SetShapeOffset(Vector2 offset)
        {
            shapeOffset = offset;
            return this;
        }

        public HitBoxBuilder SetShape(Shape2D shape)
        {
            this.shape = shape;
            return this;
        }

        public HitBox Build()
        {
            if (shape is null || owner is null)
            {
                Log.Error("Shape or owner is null");
                return null;
            }

            var collision = new CollisionShape2D
            {
                Shape = shape,
                Position = shapeOffset,
            };

            var hitBox = new HitBox
            {
                GlobalPosition = position,
                Rotation = rotation,
                Type = type,
                Damage = damage,
                HitBoxOwner = owner,
            };

            duration = Mathf.Max(duration, 0.1f);

            hitBox.AddChild(collision);

            GameManager.CurrentScene.AddChild(hitBox);
            GameManager.CurrentScene.GetTree().CreateTimer(duration).Timeout += () =>
            {
                if (!hitBox.IsInsideTree()) return;

                hitBox.QueueFree();
            };

            return hitBox;
        }
    }

    public class AttackBuilder(Entity source)
    {
        private const float CRITICAL_CHANCE = 0.2f;

        private float damage;
        private Attack.Type type;
        private Attack.KnockbackInfo knockback;
        private readonly List<StatusEffect.Info> statusEffectPool = [];

        public AttackBuilder SetDamage(float damage)
        {
            this.damage = damage;
            return this;
        }

        public AttackBuilder SetType(Attack.Type type)
        {
            this.type = type;
            return this;
        }

        public AttackBuilder SetKnockback(Vector2 direction, float force)
        {
            if (force <= 0) return this;

            knockback = new Attack.KnockbackInfo { Direction = direction, Force = force };
            return this;
        }

        public AttackBuilder SetKnockback(float force)
        {
            if (force <= 0) return this;

            knockback = new Attack.KnockbackInfo { Direction = Vector2.Zero, Force = force };
            return this;
        }

        public AttackBuilder SetStatusEffectPool(IEnumerable<StatusEffect.Info> pool)
        {
            statusEffectPool.AddRange(pool);
            return this;
        }

        public Attack Build()
        {
            var effects = new List<StatusEffect>();

            foreach (var effect in statusEffectPool)
            {
                var randomNumber = MathUtil.RNG.RandfRange(0, 1);

                if (randomNumber > effect.Chance && !effect.IsGuaranteed) continue;

                var instance = StatusEffectRegistry.Get(effect.Id);

                if (instance == null) continue;

                effects.Add(instance);
            }

            var isCritical = MathUtil.RNG.RandfRange(0, 1) < CRITICAL_CHANCE;
            var calculatedDamage = damage * (isCritical ? MathUtil.RNG.RandfRange(1.5f, 2f) : 1);
            calculatedDamage *= type == Attack.Type.Physical ? 1 : 1.2f;
            calculatedDamage = (float)Math.Round(calculatedDamage);

            var attack = new Attack
            {
                Damage = calculatedDamage,
                Source = source,
                AttackType = type,
                StatusEffects = effects,
                Knockback = knockback
            };

            return attack;
        }
    }
}