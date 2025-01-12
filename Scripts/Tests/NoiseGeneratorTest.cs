using Godot;
using System.Threading.Tasks;
using System.Linq;
using Godot.Collections;
using GodotUtilities;

namespace Game.Tests;

using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
public class NoiseGeneratorTest
{
    private const string SourceScene = "res://Scenes/Levels/Noise.tscn";

    private ISceneRunner runner;
    private Node generator;
    private Resource settings;


    [Before]
    public void Setup()
    {
        runner = ISceneRunner.Load(SourceScene);
        generator = runner.FindChild("NoiseGenerator");
        settings = (Resource)generator.Get("settings");
        generator.Set("random_seed", false);
        generator.Set("seed", 12345);
    }

    [After]
    public void TearDown()
    {
        runner?.Dispose();
        runner = null;
        generator = null;
        settings = null;
    }

    [TestCase]
    public void TestGeneratorInitialization()
    {
        AssertThat(generator).IsNotNull();
        AssertThat(settings).IsNotNull();
        AssertThat(settings.Get("noise")).IsNotNull();
    }

    [TestCase]
    public void TestBasicNoiseGeneration()
    {
        var grid = (Resource)generator.Call("get_grid");

        AssertThat(grid).IsNotNull();

        var values = ((Array)grid.Call("get_values", 0)).Count;
        var worldSize = (Vector2I)settings.Get("world_size");

        AssertThat(values).IsEqual(worldSize.X * worldSize.Y);
    }

    // [TestCase]
    // public void TestNoiseThresholds()
    // {
    //     var tileData = (Array)settings.Get("tiles");
    //     var worldSize = (Vector2I)settings.Get("world_size");
    //
    //     foreach (Resource tile in tileData)
    //     {
    //         var foundMatch = false;
    //
    //         for (var x = 0; x < worldSize.X; x++)
    //         {
    //             for (var y = 0; y < worldSize.Y; y++)
    //             {
    //                 var grid = (Resource)generator.Call("get_grid");
    //                 var value = grid.Call("get_value", x, y, 0);
    //                 var data = tile.Get("tile");
    //
    //                 if (!value.Equals(data)) continue;
    //
    //                 foundMatch = true;
    //                 break;
    //             }
    //
    //             if (foundMatch) break;
    //         }
    //     }
    // }

    [TestCase]
    public async Task TestGeneratesTheSameMap()
    {
        var values = await GenerateForComparison();

        var allSame = values.Item1.Where((t, i) => !t.Equals(values.Item2[i])).Any();

        AssertBool(allSame).IsTrue();
    }

    [TestCase]
    public async Task TestGeneratesDifferentMap()
    {
        var values = await GenerateForComparison(true);
        var allSame = !values.Item1.Where((t, i) => !t.Equals(values.Item2[i])).Any();

        AssertBool(allSame).IsFalse();
    }

    private async Task<(Array, Array)> GenerateForComparison(bool random = false)
    {
        if (random) generator.Set("seed", MathUtil.RNG.Randi());
        generator.Call("generate");

        await Task.Delay(1000);

        var grid = (Resource)generator.Call("get_grid");
        var values = (Array)grid.Call("get_values", 0);

        if (random) generator.Set("seed", MathUtil.RNG.Randi());
        generator.Call("generate");

        await Task.Delay(1000);

        var values2 = (Array)grid.Call("get_values", 0);

        return (values, values2);
    }
}