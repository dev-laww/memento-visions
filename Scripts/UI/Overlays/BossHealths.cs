using System.Collections.Generic;
using Game.Autoload;
using Game.Entities;
using Game.UI.Common;
using Godot;
using GodotUtilities;

namespace Game.UI.Overlays;

[Scene]
public partial class BossHealths : CanvasLayer
{
    [Node] private VBoxContainer healthBarsContainer;
    [Node] private ResourcePreloader resourcePreloader;

    private Dictionary<Enemy, BossHealthBar> healthBars = [];

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }


    public override void _Ready()
    {
        healthBarsContainer.QueueFreeChildren();

        EnemyManager.EnemyRegistered += OnEnemyRegistered;
        EnemyManager.EnemyUnregistered += OnEnemyUnregistered;

        foreach (var enemy in EnemyManager.EnemiesOfType(Enemy.EnemyType.Boss))
        {
            OnEnemyRegistered(enemy);
        }
    }

    private void OnEnemyRegistered(Enemy enemy)
    {
        if (enemy.Type != Enemy.EnemyType.Boss) return;

        var healthBar = resourcePreloader.InstanceSceneOrNull<BossHealthBar>();
        if (healthBar == null) return;

        healthBarsContainer.AddChild(healthBar);

        healthBar.BossName = enemy.BossName;
        healthBar.HealthBar.Initialize(enemy.StatsManager);
        healthBars[enemy] = healthBar;
    }

    private void OnEnemyUnregistered(Enemy enemy)
    {
        if (healthBars.TryGetValue(enemy, out var healthBar))
        {
            healthBar.QueueFree();
            healthBars.Remove(enemy);
        }
    }
}
