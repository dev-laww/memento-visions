using Godot;
using Game.Autoload;
using Game.Components;

namespace Game.Data;

public partial class Toughness : StatusEffect
{
    [Export] public float Amount = 10f;
    [Export] public StatsManager.ModifyMode Mode = StatsManager.ModifyMode.Value;

    public override void Apply()
    {
        TargetStatsManager?.IncreaseDefense(Amount, Mode);
        FloatingTextManager.SpawnFloatingText("Tough", Target.GlobalPosition);
    }

    public override void Remove()
    {
        TargetStatsManager?.DecreaseDefense(Amount, Mode);
    }
}