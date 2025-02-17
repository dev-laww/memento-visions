using System.Collections.Generic;
using Game.Common;
using Game.Entities;
using Godot;

namespace Game.AutoLoad;

public partial class EnemyManager : AutoLoad<EnemyManager>
{
    [Signal] public delegate void EnemySpawnedEventHandler(Entity.SpawnInfo info);
    [Signal] public delegate void EnemyDiedEventHandler(Entity.DeathInfo info);

    private readonly List<Enemy> enemies = [];

    public static IReadOnlyList<Enemy> Enemies => Instance.enemies;
    public static int EnemyCount => Instance.enemies.Count;

    public static void Register(Entity.SpawnInfo info)
    {
        if (info.Entity is not Enemy enemy)
        {
            Log.Error($"Cannot register {info.Entity} as an enemy.");
            return;
        }

        Instance.enemies.Add(enemy);
        Instance.EmitSignalEnemySpawned(info);

        Log.Debug($"{enemy} added to the registry. {info}");
    }

    public static void Unregister(Entity.DeathInfo info)
    {
        Instance.enemies.Remove(info.Victim as Enemy);
        Instance.EmitSignalEnemyDied(info);

        Log.Debug($"{info.Victim} removed from the registry. {info}");
    }
}