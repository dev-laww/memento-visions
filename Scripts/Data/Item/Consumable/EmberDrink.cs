using Game.Common;
using Game.Components;
using Game.Entities;
using Godot;

namespace Game.Data;

[Tool]
public partial class EmberDrink : Item
{
    public override void Use(Entity entity)
    {
        if (StatusEffectRegistry.Get("strength") is not Strength strength)
        {
            Log.Error("Strength status effect not found");
            return;
        }
        
        strength.Buff = 15f;
        strength.Mode = StatsManager.ModifyMode.Percentage;
        strength.Duration = 300f;

        entity.StatsManager.Heal(10, mode: StatsManager.ModifyMode.Percentage);
        entity.StatsManager.AddStatusEffect(strength);
    }
}