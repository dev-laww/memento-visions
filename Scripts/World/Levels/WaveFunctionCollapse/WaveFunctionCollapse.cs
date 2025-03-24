using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Game.Common.Extensions;
using Game.Components;
using Game.Generation;
using Godot;
using GodotUtilities;

namespace Game.Levels;

[Tool]
[Scene]
public partial class WaveFunctionCollapse : Node2D
{
    private const int MAX_ITERATIONS = 10000;

    [Export] private WaveFunctionCollapseSettings settings;
    [Export] private bool generateOnReady = true;

    [Node] private Node2D map;
    [Node] private NavigationManager navigationManager;

    private readonly Dictionary<Vector2I, string> directions = new()
    {
        { Vector2I.Right, "Right" },
        { Vector2I.Left, "Left" },
        { Vector2I.Up, "Up" },
        { Vector2I.Down, "Down" }
    };

    private readonly Dictionary<Vector2I, List<WaveFunctionCollapseEntry>> waveFunction = [];
    private Grid<PackedScene> grid;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        if (generateOnReady)
            Generate();
    }

    private void InitializeWaveFunction()
    {
        Clear();

        grid = new Grid<PackedScene>(settings.GridSize, Vector2I.Zero);

        waveFunction.Clear();

        for (var x = 0; x < settings.GridSize.X; x++)
        {
            for (var y = 0; y < settings.GridSize.Y; y++)
            {
                waveFunction[new Vector2I(x, y)] = [.. settings.Entries];
            }
        }
    }

    public void Generate()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        InitializeWaveFunction();

        var iterations = 0;
        while (!IsCollapsed() && iterations < MAX_ITERATIONS)
        {
            iterations++;
            var coords = GetLowestEntropyCoords();
            Collapse(coords);
            Propagate(coords);
        }

        if (iterations == MAX_ITERATIONS)
        {
            GD.PushError("Generation reached max iterations.");
            return;
        }

        foreach (var (pos, v) in waveFunction)
        {
            if (v.Count <= 0) continue;

            grid[pos] = v[0].Scene;
        }

        // place the packed scenes in the world
        for (var x = 0; x < settings.GridSize.X; x++)
        {
            for (var y = 0; y < settings.GridSize.Y; y++)
            {
                var scene = grid[new Vector2I(x, y)];
                if (scene is null) continue;

                var instance = scene.Instantiate<Node2D>();
                instance.Position = new Vector2(x * settings.CellSize, y * settings.CellSize);

                map.EditorAddChild(instance);
            }
        }

        stopwatch.Stop();

        if (OS.IsDebugBuild())
        {
            GD.Print($"Generation took {stopwatch.ElapsedMilliseconds}ms");
        }

        GetTree().CreateTimer(0.2).Timeout += navigationManager.PlaceNavigationRegions;
    }

    public void Clear()
    {
        foreach (var child in map.GetChildren())
        {
            child.QueueFree();
        }

        navigationManager.Clear();
    }

    private void Collapse(Vector2I coords)
    {
        var entries = waveFunction[coords];
        if (entries.Count == 0) return;

        float totalWeight = entries.Sum(e => e.Weight);
        float rand = MathUtil.RNG.Randf() * totalWeight;

        WaveFunctionCollapseEntry chosen = null;

        foreach (var entry in entries)
        {
            rand -= entry.Weight;
            if (rand <= 0)
            {
                chosen = entry;
                break;
            }
        }

        waveFunction[coords] = [chosen];
    }

    private bool IsCollapsed()
    {
        return waveFunction.Values.All(entries => entries.Count == 1);
    }

    private void Propagate(Vector2I coords)
    {
        var stack = new Stack<Vector2I>();
        stack.Push(coords);

        while (stack.Count > 0)
        {
            var currentCoords = stack.Pop();
            var entries = waveFunction[currentCoords];

            foreach (var dir in directions.Keys)
            {
                var otherCoords = currentCoords + dir;
                if (!waveFunction.ContainsKey(otherCoords)) continue;

                var possibleNeighbors = GetPossibleNeighbors(currentCoords, dir);
                if (!possibleNeighbors.Any()) continue;

                var otherPossibleEntries = waveFunction[otherCoords].ToList();
                var prevCount = otherPossibleEntries.Count;

                waveFunction[otherCoords] = [.. otherPossibleEntries.Where(possibleNeighbors.Contains)];

                if (prevCount != waveFunction[otherCoords].Count && !stack.Contains(otherCoords))
                {
                    stack.Push(otherCoords);
                }
            }
        }
    }

    private List<WaveFunctionCollapseEntry> GetPossibleNeighbors(Vector2I coords, Vector2I direction)
    {
        var possibleNeighbors = new List<WaveFunctionCollapseEntry>();
        var directionKey = directions[direction];
        var neighborCoords = coords + direction;

        if (!waveFunction.TryGetValue(neighborCoords, out List<WaveFunctionCollapseEntry> value))
            return possibleNeighbors;

        foreach (var entry in waveFunction[coords])
        {
            var validNeighbors = directionKey switch
            {
                "Right" => entry.Right,
                "Left" => entry.Left,
                "Up" => entry.Up,
                "Down" => entry.Down,
                _ => []
            };

            foreach (var otherEntry in value)
            {
                if (validNeighbors.Any(n => n.ResourcePath == otherEntry.Scene.ResourcePath))
                {
                    possibleNeighbors.Add(otherEntry);
                }
            }
        }

        return possibleNeighbors;
    }

    private Vector2I GetLowestEntropyCoords()
    {
        var lowestEntropy = float.MaxValue;
        var lowestEntropyCoords = Vector2I.Zero;

        foreach (var (coords, entries) in waveFunction)
        {
            if (entries.Count == 1) continue;

            var entropy = entries.Count + (MathUtil.RNG.Randf() / 1000);
            if (entropy >= lowestEntropy) continue;

            lowestEntropy = entropy;
            lowestEntropyCoords = coords;
        }

        return lowestEntropyCoords;
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (settings is null)
            return ["Settings is not set"];

        if (settings.Entries.Length == 0)
            warnings.Add("Entries is empty");

        return [.. warnings];
    }
}