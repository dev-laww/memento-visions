using System.Collections.Generic;
using Game.Resources;
using Godot;

namespace Game.Components.Area;

[Tool]
[GlobalClass]
public partial class HitBox : Area2D
{
    private float _damage;

    private Stats resource;

    [Export]
    private Stats Stats
    {
        get => resource;
        set
        {
            resource = value;
            UpdateConfigurationWarnings();
        }
    }

    public float Damage
    {
        get => _damage;
        set
        {
            _damage = value;

            if (value is <= 0 or float.NaN)
                return;

            if (value > _damage)
                EmitSignal(SignalName.DamageIncreased, _damage - value);
            else
                EmitSignal(SignalName.DamageDecreased, value - _damage);
        }
    }

    [Signal]
    public delegate void DamageIncreasedEventHandler(float damage);

    [Signal]
    public delegate void DamageDecreasedEventHandler(float damage);

    public override void _Ready()
    {
        Damage = Stats.Attack;
        CollisionLayer = 1 << 10;
        CollisionMask = 1 << 11;
        NotifyPropertyListChanged();
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (Stats == null)
            warnings.Add("Stats resource not found.");

        return warnings.ToArray();
    }
}