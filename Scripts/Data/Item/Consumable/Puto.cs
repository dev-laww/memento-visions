using Game.Common;
using Game.Components;
using Game.Entities;
using Godot;

namespace Game.Data;

[Tool]
public partial class Puto : Item
{
    public override void Use(Entity entity)
    {
        if (StatusEffectRegistry.Get("toughness") is not Toughness toughness)
        {
            Log.Error("Status effect 'toughness' not found in registry.");
            return;
        }

        toughness.Amount = 15;
        toughness.Duration = 10;
        toughness.Mode = StatsManager.ModifyMode.Percentage;

        entity.StatsManager.AddStatusEffect(toughness);
        entity.StatsManager.Heal(10, mode: StatsManager.ModifyMode.Percentage);
    }
}