using Game.Common;
using Game.Components;
using Game.Entities;
using Godot;

namespace Game.Data;

[Tool]
public partial class BreezePotion : Item
{
    public override void Use(Entity entity)
    {
        if (StatusEffectRegistry.Get("swiftness") is not Swiftness effect)
        {
            Log.Error("Status effect 'swiftness' not found in registry.");
            return;
        }

        effect.SpeedAmount = .1f;

        entity.StatsManager.AddStatusEffect(effect);
        entity.StatsManager.Heal(10, mode: StatsManager.ModifyMode.Percentage);
    }
}