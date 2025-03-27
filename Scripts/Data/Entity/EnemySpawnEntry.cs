using Game.Common;
using Game.Entities;
using Godot;
using GodotUtilities;

namespace Game.Data;

[Tool]
[GlobalClass]
public partial class EnemySpawnEntry : Resource
{
    [Export] private PackedScene enemyScene;
    [Export] private float spawnChance = 0.5f;
    [Export] public int Weight = 1;

    [ExportGroup("Thresholds")]
    [Export(PropertyHint.Range, "-1,1")]
    public float Min
    {
        get => min;
        set
        {
            min = value;
            EmitChanged();
        }
    }

    [Export(PropertyHint.Range, "-1,1")]
    public float Max
    {
        get => max;
        set
        {
            max = value;
            EmitChanged();
        }
    }

    public FastNoiseLite Noise
    {
        get => noise;
        set
        {
            noise = value;

            if (!noise.IsConnected("changed", Callable.From(EmitChanged)))
            {
                noise.Connect("changed", Callable.From(EmitChanged));
            }
        }
    }

    private FastNoiseLite noise;
    private float min = -1;
    private float max = 1;

    public Enemy Create(Vector2 position)
    {
        var randomValue = MathUtil.RNG.RandfRange(0, 1);
        var noiseValue = noise.GetNoise2D(position.X, position.Y);

        if (randomValue > spawnChance || noiseValue < min || noiseValue > max)
        {
            return null;
        }

        if (enemyScene == null)
        {
            Log.Error("Enemy scene is null.");
            return null;
        }

        var enemy = enemyScene.InstantiateOrNull<Enemy>();

        enemy.Position = position;

        return enemy;
    }
}