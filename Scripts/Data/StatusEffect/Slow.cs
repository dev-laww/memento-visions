using Godot;
using Game.Common;
using Game.Autoload;

namespace Game.Data;

public partial class Slow : StatusEffect
{
    [Export] public float SlowAmount = 0.5f;

    public override void Apply()
    {
        TargetStatsManager?.ApplySpeedModifier(Id, -SlowAmount);
        FloatingTextManager.SpawnFloatingText("Slowed", Target.GlobalPosition);
    }

    public override void Remove()
    {
        TargetStatsManager?.RemoveSpeedModifier(Id, -SlowAmount);
    }

    public override void Stack(int amount = 1)
    {
        StackCount += amount;
        RemainingDuration += Duration * amount;
        Log.Debug($"{this} stacked to {StackCount} {RemainingDuration}");
    }
}

