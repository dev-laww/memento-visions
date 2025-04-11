using System.Collections.Generic;
using Game.Utils.Battle;
using Godot;
using Game.Autoload;
using Game.Entities;

namespace Game.Components;

[Tool]
[GlobalClass, Icon("res://assets/icons/health_number_manager.svg")]
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
        if (StatsManager == null || Engine.IsEditorHint()) return;

        StatsManager.AttackReceived += OnAttackReceived;
        StatsManager.StatIncreased += OnStatIncreased;
        StatsManager.DamageTaken += OnDamageTaken;
    }

    private void OnStatIncreased(float value, StatsType type)
    {
        if (type != StatsType.Health)
            return;
        value = Mathf.Max(0, Mathf.RoundToInt(value));
        var args = new FloatingTextManager.FloatingTextSpawnArgs
        {
            Text = $"+{value}",
            Position = (Owner as Node2D)?.GlobalPosition ?? Vector2.Zero,
            Parent = GetParent(),
            Color = new Color(0.5f, 1f, 0.5f),
            SpawnRadius = 16
        };

        var text = FloatingTextManager.SpawnFloatingText(args);
        text.Finished += text.QueueFree;
    }

    private void OnDamageTaken(float value)
    {
        value = Mathf.Max(0, Mathf.RoundToInt(value));
        var args = new FloatingTextManager.FloatingTextSpawnArgs
        {
            Text = $"-{value}",
            Position = (Owner as Node2D)?.GlobalPosition ?? Vector2.Zero,
            Color = new Color(1f, 0.5f, 0.5f),
            Parent = GetParent(),
            SpawnRadius = 16
        };

        var text = FloatingTextManager.SpawnFloatingText(args);
        text.Finished += text.QueueFree;
    }

    private void OnAttackReceived(Attack attack)
    {
        var willNegate = StatsManager.Defense >= attack.Damage;

        if (willNegate)
        {
            var args = new FloatingTextManager.FloatingTextSpawnArgs
            {
                Text = "Defended", // TODO: think of a better text
                Position = (Owner as Node2D)?.GlobalPosition ?? Vector2.Zero,
                Color = Colors.Gray,
                Parent = Owner,
                SpawnRadius = 16
            };

            var text = FloatingTextManager.SpawnFloatingText(args);
            text.Finished += text.QueueFree;
            return;
        }

        var damage = Mathf.Max(1, Mathf.RoundToInt(attack.Damage));
        var floatingText = FloatingTextManager.SpawnDamageText(
            Owner,
            (Owner as Node2D)?.GlobalPosition ?? Vector2.Zero,
            damage - StatsManager.Defense
        );

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