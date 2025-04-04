using System;
using System.Collections.Generic;
using System.Linq;
using Game.Common;
using Game.Entities;
using Game.UI.Common;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.Autoload;

[Scene]
public partial class EnemyManager : Autoload<EnemyManager>
{
    [Node] private VBoxContainer healthBarsContainer;
    [Node] private ResourcePreloader resourcePreloader;

    private readonly Dictionary<Enemy, BossHealthBar> healthBars = [];
    private readonly List<Enemy> enemies = [];

    public static IReadOnlyList<Enemy> Enemies => Instance.enemies;
    public static int EnemyCount => Instance.enemies.Count;
    public static event Action<Enemy> EnemyRegistered;
    public static event Action<Enemy> EnemyUnregistered;
    public static event Action<int> EnemyCountChanged;

    public static IReadOnlyList<Enemy> EnemiesOfType(Enemy.EnemyType type) => [.. Instance.enemies.Where(enemy => enemy.Type == type)];

    public static void Register(Entity.SpawnInfo info)
    {
        if (info.Entity is not Enemy enemy)
        {
            Log.Error($"Cannot register {info.Entity} as an enemy.");
            return;
        }

        info.Entity.AddToGroup();
        info.Entity.AddToGroup("Enemy");
        Instance.enemies.Add(enemy);
        EnemyRegistered?.Invoke(enemy);
        Instance.OnEnemyRegistered(enemy);
        EnemyCountChanged?.Invoke(Instance.enemies.Count);
        enemy.StatsManager.SetLevel(Instance.GetPlayer()?.StatsManager.Level ?? 1);

        Log.Debug($"{enemy} added to the registry. {info}");
    }

    public static void Unregister(Entity.DeathInfo info)
    {
        Instance.enemies.Remove(info.Victim as Enemy);
        Instance.OnEnemyUnregistered(info.Victim as Enemy);
        EnemyUnregistered?.Invoke(info.Victim as Enemy);
        EnemyCountChanged?.Invoke(Instance.enemies.Count);

        Log.Debug($"{info.Victim} removed from the registry. {info}");
    }

    public static void Unregister(Enemy enemy)
    {
        Instance.enemies.Remove(enemy);
        Instance.OnEnemyUnregistered(enemy);
        EnemyUnregistered?.Invoke(enemy);
    }

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        healthBarsContainer.QueueFreeChildren();

        foreach (var enemy in EnemiesOfType(Enemy.EnemyType.Boss))
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