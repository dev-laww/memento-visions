using System.Collections.Generic;
using Game.Utils.Battle;
using Godot;
using GodotUtilities;
using Game.Globals;
using Game.Entities;
using Game.Common;

namespace Game.Components;

[Tool]
[Scene]
[GlobalClass, Icon("res://assets/icons/health-number-manager.svg")]
public partial class HealthNumberManager : Node
{
    [Export]
    private StatsManager StatsManager
    {
        get => manager;
        set
        {
            manager = value;
            UpdateConfigurationWarnings();
        }
    }

    private StatsManager manager;

    public override void _Ready()
    {
        if (StatsManager != null)
            StatsManager.AttackReceived += OnAttackReceived;
    }

    private void OnAttackReceived(Attack attack)
    {
        Log.Debug($"Attack received: {attack} from {attack.Attacker}");
        var floatingText = FloatingTextManager.SpawnDamageText(Owner, (Owner as Node2D).GlobalPosition, attack.Damage);

        floatingText.SetColor(attack.Critical ? new Color(1, 0.5f, 0.5f) : attack.AttackType switch
        {
            Attack.Type.Physical => Colors.White,
            Attack.Type.Magical => new Color(0.5f, 0.5f, 1),
            _ => Colors.White
        });
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (manager == null)
            warnings.Add("StatsManager is not set.");

        if (GetParent() is not Entity)
            warnings.Add("HealthNumberManager should be a child of an Entity.");

        return [.. warnings];
    }
}