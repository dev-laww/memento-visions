using System.Linq;
using Game.Data;
using Godot;
using Godot.Collections;
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
        }
    }

    public Array<Vector2> SpawnPoints = [];
    private FastNoiseLite noise = new();
    private Array<EnemySpawnEntry> spawnEntries = [];
    private LootTable<EnemySpawnEntry> lootTable = new();
}