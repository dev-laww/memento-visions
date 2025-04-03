using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Game.Data;
using Godot;
using Godot.Collections;
using GodotUtilities.Logic;

namespace Game.Components;


[Tool]
[GlobalClass]
public partial class Spawner : Node
{
    [Export] private Node spawnRoot;

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

    private Array<Vector2> spawnPositions = [];
    private FastNoiseLite noise = new();
    private Array<EnemySpawnEntry> spawnEntries = [];
    private LootTable<EnemySpawnEntry> lootTable = new();

    public void SetSpawnPositions(Array<Vector2> positions) => spawnPositions = positions;

    public void Spawn()
    {
        foreach (var point in spawnPositions)
        {
            var entry = lootTable.PickItem();
            var enemy = entry.Create(point);

            if (enemy is null) continue;

            spawnRoot.AddChild(enemy);
        }
    }

    public void SpawnBoss(Vector2? position = null)
    {
        var boss = EnemyRegistry.PickRandomBoss();

        GD.Print("Spawning boss: ", boss?.Name);

        if (boss is null) return;

        position ??= spawnPositions.PickRandom();
        boss.Position = (Vector2)position;

        spawnRoot.AddChild(boss);
    }
}