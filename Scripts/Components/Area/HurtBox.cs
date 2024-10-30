using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

namespace Game.Components.Area;

[Tool]
[GlobalClass]
public partial class HurtBox : Area2D
{
    [Export]
    private StatsManager StatsManager
    {
        get => statsManager;
        set
        {
            statsManager = value;
            UpdateConfigurationWarnings();
        }
    }

    [Signal]
    public delegate void DamageReceivedEventHandler(float damage);
    
    private StatsManager statsManager;

    public override void _Ready()
    {
        AddToGroup("HurtBox");
        AreaEntered += OnHurtBoxAreaEntered;
        CollisionLayer = 1 << 11;
        CollisionMask = 1 << 10;
        NotifyPropertyListChanged();
    }

    public void ReceiveDamage(float damage)
    {
        EmitSignal(SignalName.DamageReceived, damage);
        statsManager.TakeDamage(damage);
    }

    private void OnHurtBoxAreaEntered(Area2D area)
    {
        if (area is not HitBox hitBox) return;

        if (hitBox.Owner == Owner) return;

        ReceiveDamage(hitBox.Damage);
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();
        
        if (statsManager == null)
            warnings.Add("StatsManager is not set.");

        return warnings.ToArray();
    }
}