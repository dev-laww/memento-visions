using Game.Components;
using Game.Entities;
using Game.Utils.Battle;
using Godot;


namespace Game.Utils;

public static class DamageFactory
{
    private static readonly PackedScene LineDamageScene = ResourceLoader.Load<PackedScene>("res://Scenes/Components/Battle/Damage/LineDamage.tscn");
    private static readonly PackedScene CircleDamageScene = ResourceLoader.Load<PackedScene>("res://Scenes/Components/Battle/Damage/CircleDamage.tscn");

    public class LineDamageBuilder(Vector2 start, Vector2 end)
    {
        private readonly Vector2 start = start;
        private readonly Vector2 end = end;
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
}
