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

    public Attack(float damage, Type type, bool critical)
    {
        Damage = damage;
        AttackType = type;
        IsCritical = critical;
    }

    public void RollCritical() => IsCritical = MathUtil.RNG.RandiRange(0, 100) > 80;

    public static Attack Physical(float damage, bool critical = false) => new(damage, Type.Physical, critical);

    public static Attack Magic(float damage, bool critical = false) => new(damage, Type.Magical, critical);

    public float CalculateDamage(float defense, float physicalMultiplier, float magicMultiplier)
    {
        RollCritical();
        
        var damage = Damage - defense * (AttackType == Type.Physical ? 1 : 0.8f);

        if (IsCritical)
            damage += damage * (Damage / 3);
        
        IsCritical = false;
        
        return damage * (AttackType == Type.Physical ? physicalMultiplier : magicMultiplier);
    }
}