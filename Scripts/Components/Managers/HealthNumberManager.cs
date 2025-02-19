using System.Collections.Generic;
using Game.Utils.Battle;
using Godot;
using Game.Autoload;
using Game.Entities;

namespace Game.Components;

[Tool]
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
        {
            StatsManager.AttackReceived += OnAttackReceived;
            StatsManager.StatIncreased += OnStatIncreased;
            StatsManager.DamageTaken += OnDamageTaken;
        }
    }

    private void OnStatIncreased(float value, StatsType type)
    {
        if (type != StatsType.Health)
            return;

        var args = new FloatingTextManager.FloatingTextSpawnAgrs
        {
            Text = $"+{value}",
            Position = (Owner as Node2D).GlobalPosition,
            Parent = GetParent(),
            Color = new Color(0.5f, 1f, 0.5f),
            SpawnRadius = 16
        };

        var text = FloatingTextManager.SpawnFloatingText(args);
        text.Finished += text.QueueFree;
    }

    private void OnDamageTaken(float value)
    {
        var args = new FloatingTextManager.FloatingTextSpawnAgrs
        {
            Text = $"-{value}",
            Position = (Owner as Node2D).GlobalPosition,
            Color = new Color(1f, 0.5f, 0.5f),
            Parent = GetParent(),
            SpawnRadius = 16
        };

        var text = FloatingTextManager.SpawnFloatingText(args);
        text.Finished += text.QueueFree;
    }

    private void OnAttackReceived(Attack attack)
    {
        var floatingText = FloatingTextManager.SpawnDamageText(Owner, (Owner as Node2D).GlobalPosition, attack.Damage);

        floatingText.SetColor(attack.AttackType switch
        {
            Attack.Type.Physical => Colors.White,
            Attack.Type.Magical => new Color(0.5f, 0.5f, 1),
            _ => Colors.White
        });

        if (attack.Critical)
            floatingText.ApplyTemporaryColor(new Color(1, 0.5f, 0.5f), duration: 0.3f);
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