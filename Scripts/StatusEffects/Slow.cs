using Game.Common;
using Game.Components;
using Godot;

namespace Game;

public partial class Slow : StatusEffect
{
    [Export] public float SlowAmount = 0.5f;

    public override void Apply()
    {
        Target?.StatsManager?.ApplySpeedModifier(Id, -SlowAmount);
    }

    public override void Remove()
    {
        Target?.StatsManager?.RemoveSpeedModifier(Id, -SlowAmount);
    }

    public override void Stack(int amount = 1)
    {
        StackCount += amount;
        RemainingDuration += Duration * amount;
        Log.Debug($"{this} stacked to {StackCount} {RemainingDuration}");
    }
}

