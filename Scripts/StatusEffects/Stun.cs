using Game.Common;
using Game.Components;

namespace Game.StatusEffects;

public partial class Stun : StatusEffect
{
    public override void Apply()
    {
        TargetVelocityManager?.Decelerate(force: true);
        Target?.SetPhysicsProcess(false);
        Target?.SetProcess(false);
    }

    public override void Remove()
    {
        Target?.SetPhysicsProcess(true);
        Target?.SetProcess(true);
    }
}
