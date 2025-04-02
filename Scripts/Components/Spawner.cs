using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Common.Extensions;
using Game.Data;
using Game.Entities;
using Godot;
using Godot.Collections;
using GodotUtilities;
using GodotUtilities.Logic;

namespace Game.Components;


[Tool]
[GlobalClass]
public partial class Spawner : Node
{
    [Export]
    private FastNoiseLite Noise
    {
        get => noise;
        set
        {
            noise = value;

            foreach (var entry in spawnEntries)
            {
                entry.Noise = noise;
            }
        }
    }

    [Export]
    private Array<EnemySpawnEntry> SpawnEntries
    {
        get => spawnEntries;
        set
        {
            spawnEntries = value;

            if (value.Count > 0 && value.Last() is null)
            {
                var entry = new EnemySpawnEntry
                {
                    ResourceLocalToScene = true
                };

                spawnEntries[^1] = entry;
            }

            foreach (var entry in spawnEntries)
            {
                entry.Noise = noise;
            }

            lootTable = new();

            foreach (var entry in spawnEntries)
            {
                lootTable.AddItem(entry, entry.Weight);
            }
        }
    }

    public Array<Vector2> SpawnPoints { get; } = [];
    private FastNoiseLite noise = new();
    private Array<EnemySpawnEntry> spawnEntries = [];
    private LootTable<EnemySpawnEntry> lootTable = new();


    public IEnumerable<Enemy> Spawn()
    {
        foreach (var point in SpawnPoints)
        {
            var entry = lootTable.PickItem();
            var enemy = entry.Create(point);

            if (enemy is null) continue;

            yield return enemy;
        }
    }

    public IEnumerable<Enemy> SpawnBossWave()
    {
        // TODO: Implement boss wave spawning logic

        yield break;
    }
}