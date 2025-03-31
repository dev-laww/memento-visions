using System;
using System.Collections.Generic;
using System.Linq;
using Game.Common;
using Game.Common.Utilities;
using Game.Entities;
using Godot;
using GodotUtilities;

namespace Game.Autoload;

public partial class EnemyManager : Autoload<EnemyManager>
{
    private readonly List<Enemy> enemies = [];

    public static IReadOnlyList<Enemy> Enemies => Instance.enemies;
    public static int EnemyCount => Instance.enemies.Count;
    public static event Action<Enemy> EnemyRegistered;
    public static event Action<Enemy> EnemyUnregistered;

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

        Log.Debug($"{enemy} added to the registry. {info}");
    }

    public static void Unregister(Entity.DeathInfo info)
    {
        Instance.enemies.Remove(info.Victim as Enemy);
        EnemyUnregistered?.Invoke(info.Victim as Enemy);

        Log.Debug($"{info.Victim} removed from the registry. {info}");
    }
}