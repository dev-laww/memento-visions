using System.Collections.Generic;
using Game.Data;
using Game.Entities;
using Godot;

namespace Game.Utils.Battle;

public partial class Attack : RefCounted
{
    public class KnockbackInfo
    {
        public Vector2 Direction;
        public float Force;

        public void Deconstruct(out Vector2 direction, out float force)
        {
            direction = Direction;
            force = Force;
        }
    }

    public enum Type
    {
        Physical,
        Magical
    }

    public float Damage;

    public Type AttackType;

    public Entity Source;

    public KnockbackInfo Knockback;

    public bool Critical;

    public List<StatusEffect> StatusEffects;

    public bool HasStatusEffects => StatusEffects.Count > 0;
}