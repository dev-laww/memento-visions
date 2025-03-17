using Game.Common;
using Godot;

namespace Game.Components;

public partial class Bleed : StatusEffect
{
    [Export] private float damagePerSecond = 2;

    private float accumulatedDamage;

    protected override void Tick()
    {
        accumulatedDamage += damagePerSecond * (float)GetPhysicsProcessDeltaTime();

        if (accumulatedDamage < 1) return;

        Target?.StatsManager.TakeDamage(accumulatedDamage);

        accumulatedDamage = 0;
    }

    public override void Stack(int amount = 1)
    {
        var baseDamage = damagePerSecond / StackCount;
        damagePerSecond = baseDamage * (StackCount + amount);

        StackCount += amount;

        Log.Debug($"{this} stacked to {StackCount} {RemainingDuration}");
    }
}