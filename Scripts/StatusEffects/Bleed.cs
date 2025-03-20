using Godot;

namespace Game.StatusEffects;

public partial class Bleed : StatusEffect
{
    [Export] private float damagePerSecond = 2;

    private float accumulatedDamage;

    protected override void Tick()
    {
        accumulatedDamage += damagePerSecond * (float)GetPhysicsProcessDeltaTime();

        if (accumulatedDamage < StackCount) return;

        TargetStatsManager?.TakeDamage(accumulatedDamage);

        accumulatedDamage = 0;
    }

    public override void Stack(int amount = 1)
    {
        var baseDamage = damagePerSecond / StackCount;
        damagePerSecond = baseDamage * (StackCount + amount);

        StackCount += amount;
        RemainingDuration = Duration;
    }
}