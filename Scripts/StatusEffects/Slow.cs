using Game.Components.Battle;
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
}

