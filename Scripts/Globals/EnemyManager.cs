using System.Collections.Generic;
using Game.Common;
using Game.Entities.Enemies;
using Godot;

namespace Game.Globals;

public partial class EnemyManager : Global<EnemyManager>
{
    [Signal] public delegate void SpawnedEventHandler(Enemy enemy);
    [Signal] public delegate void DiedEventHandler(Enemy enemy);

    public static event SpawnedEventHandler EnemySpawned
    {
        add => Instance.Spawned += value;
        remove => Instance.Spawned -= value;
    }

    public static event DiedEventHandler EnemyDied
    {
        add => Instance.Died += value;
        remove => Instance.Died -= value;
    }

    public readonly List<Enemy> enemies = [];

    public static IReadOnlyList<Enemy> Enemies => Instance.enemies;
    public static int EnemyCount => Instance.enemies.Count;

    public static void Register(Enemy enemy)
    {
        Instance.enemies.Add(enemy);
        Instance.EmitSignal(SignalName.Spawned, enemy);
        Log.Debug($"{enemy} registered.");
    }

    public static void Unregister(Enemy enemy)
    {
        Instance.EmitSignal(SignalName.Died, enemy);
        Instance.enemies.Remove(enemy);
        Log.Debug($"{enemy} unregistered.");
    }
}