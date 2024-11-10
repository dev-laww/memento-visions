using System.Collections.Generic;
using Game.Battle;
using Godot;

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
    public delegate void AttackReceivedEventHandler(float damage, Attack.Type type, bool isCritical = false);

    private StatsManager statsManager;

    public override void _Ready()
    {
        AddToGroup("HurtBox");
        AreaEntered += OnHurtBoxAreaEntered;
        CollisionLayer = 1 << 11;
        CollisionMask = 1 << 10;
        NotifyPropertyListChanged();
    }

    public void ReceiveAttack(HitBox hitBox)
    {
        var attack = hitBox.Attack;
        attack = hitBox.Type switch
        {
            Attack.Type.Physical => attack.Roll(StatsManager.Defense, StatsManager.PhysicalDamageMultiplier),
            Attack.Type.Magical => attack.Roll(StatsManager.Defense, StatsManager.MagicalDamageMultiplier),
            _ => attack.Roll(StatsManager.Defense)
        };

        statsManager.TakeDamage(attack.Damage);

        EmitSignal(SignalName.AttackReceived, attack.Damage, (int)attack.AttackType, attack.IsCritical);
    }

    private void OnHurtBoxAreaEntered(Area2D area)
    {
        if (area is not HitBox hitBox) return;

        if (hitBox.Owner == Owner) return;

        ReceiveAttack(hitBox);
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (statsManager == null)
            warnings.Add("StatsManager is not set.");

        return warnings.ToArray();
    }
}