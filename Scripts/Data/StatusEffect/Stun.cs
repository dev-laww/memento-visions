using Game.Autoload;

namespace Game.Data;

public partial class Stun : StatusEffect
{
    public override void Apply()
    {
        TargetVelocityManager?.Decelerate(force: true);
        Target?.SetPhysicsProcess(false);
        Target?.SetProcess(false);

        var text = FloatingTextManager.SpawnFloatingText("Stunned", Target.GlobalPosition);
        text.Finished += text.QueueFree;
    }

    public override void Remove()
    {
        Target?.SetPhysicsProcess(true);
        Target?.SetProcess(true);
    }

    public override void Stack(int amount = 1)
    {
        RemainingDuration = Duration;
    }
}
