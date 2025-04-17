using System.Linq;
using Game.Components;
using Game.Entities;
using Godot;
using GodotUtilities;

namespace Game.Data;

[GlobalClass, Icon("res://assets/icons/status_effect.svg")]
public partial class HealthRegen : StatusEffect
{
    [Export] public float HealthRegenPerTick = 1.0f;
    [Export] public float TickInterval = 1.0f;
    private float timeSinceLastTick = 0.0f;

    protected override void Tick()
    {
        timeSinceLastTick += (float)Target.GetPhysicsProcessDeltaTime();

        if (timeSinceLastTick >= TickInterval)
        {
            timeSinceLastTick = 0.0f;
            RegenerateHealth();
        }
    }

    private void RegenerateHealth()
    {
        if (TargetStatsManager != null)
        {
            TargetStatsManager.Heal(HealthRegenPerTick * StackCount);
        }
    }

    public override void Apply() { }

    public override void Remove() { }
    
}