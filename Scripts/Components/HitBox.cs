using System;
using System.Collections.Generic;
using Godot;

namespace Game.Components;

[Tool]
[GlobalClass]
public partial class HitBox : Area2D
{
    private float _damage;

    [Export]
    public float Damage
    {
        get => _damage;
        set
        {
            _damage = value;
            UpdateConfigurationWarnings();

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


    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (_damage is <= 0 or float.NaN)
            warnings.Add("Damage is not set or is not a finite number.");

        return warnings.ToArray();
    }
}