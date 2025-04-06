using Godot;
using Game.Autoload;

namespace Game.Data;

public partial class Swiftness : StatusEffect
{
    [Export] public float SpeedAmount = 0.5f;

    public override void Apply()
    {
        TargetStatsManager?.ApplySpeedModifier(Id, SpeedAmount);
        var text = FloatingTextManager.SpawnFloatingText("Swift", Target.GlobalPosition);
        text.Finished += text.QueueFree;
    }

    public override void Remove()
    {
        TargetStatsManager?.RemoveSpeedModifier(Id, SpeedAmount);
    }
}