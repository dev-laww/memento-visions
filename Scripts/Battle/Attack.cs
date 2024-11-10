using Godot;
using GodotUtilities;

namespace Game.Battle;

public class Attack
{
    public enum Type
    {
        Physical,
        Magical
    }

    public float Damage { get; }

    public Type AttackType { get; }

    public bool IsCritical { get; private set; }

    private Attack(float damage, Type type, bool critical)
    {
        Damage = damage;
        AttackType = type;
        IsCritical = critical;
    }

    public static Attack Physical(float damage, bool critical = false) => new(damage, Type.Physical, critical);

    public static Attack Magical(float damage, bool critical = false) => new(damage, Type.Magical, critical);

    public Attack Roll(float defense, float damageMultiplier = 1f)
    {
        var damage = Damage;
        var critical = MathUtil.RNG.RandfRange(0, 1) < 0.1f;

        if (critical)
            damage += Damage * (damage / 3);

        damage -= defense * AttackType switch
        {
            Type.Physical => 1f,
            Type.Magical => 0.8f,
            _ => 1f
        };
        damage *= damageMultiplier;

        return new Attack(damage, AttackType, critical);
    }
}