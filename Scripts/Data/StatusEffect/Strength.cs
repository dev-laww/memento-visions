using Godot;
using Game.Autoload;
using Game.Components;

namespace Game.Data;

public partial class Strength : StatusEffect
{
    [Export] public float Buff = 10f;
    [Export] public StatsManager.ModifyMode Mode = StatsManager.ModifyMode.Value;

    public override void Apply()
    {
        TargetStatsManager?.IncreaseDamage(Buff, Mode);
        var text = FloatingTextManager.SpawnFloatingText("Swift", Target.GlobalPosition);
        text.Finished += text.QueueFree;
    }

    public override void Remove()
    {
        TargetStatsManager?.DecreaseDamage(Buff, Mode);
    }
}