using System.Collections.Generic;
using Game.Battle;
using Godot;

namespace Game.Components.Area;

[Tool]
[GlobalClass]
public partial class HitBox : Area2D
{
    [Export]
    public Attack.Type Type { get; set; } = Attack.Type.Physical;

    [Export]
    public StatsManager StatsManager
    {
        get => manager;
        set
        {
            manager = value;
            UpdateConfigurationWarnings();
        }
    }

    private StatsManager manager;

    public Attack Attack => Type switch
    {
        Attack.Type.Physical => StatsManager.PhysicalAttack,
        Attack.Type.Magical => StatsManager.MagicalAttack,
        _ => StatsManager.PhysicalAttack
    };

    public override void _Ready()
    {
        CollisionLayer = 1 << 10;
        CollisionMask = 1 << 11;
        NotifyPropertyListChanged();
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (manager == null)
            warnings.Add("StatsManager is not set.");

        return warnings.ToArray();
    }
}