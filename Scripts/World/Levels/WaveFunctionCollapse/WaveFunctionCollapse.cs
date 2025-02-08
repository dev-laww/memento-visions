using System;
using System.Collections.Generic;
using System.Linq;
using Game.Generation;
using Godot;
using GodotUtilities.Logic;

namespace Game.Levels;

// TODO: Check if the algorithm is working correctly
public partial class WaveFunctionCollapse : Node2D
{
    [Export] private WaveFuncitonCollapseSettings settings;

    private const int MAX_ITERATIONS = 10000;
    private readonly Dictionary<Vector2I, string> directions = new()
    {
        { Vector2I.Right, "Right" },
        { Vector2I.Left, "Left" },
        { Vector2I.Up, "Up" },
        { Vector2I.Down, "Down" }
    };

    private Dictionary<Vector2I, List<WaveFunctionCollapseEntry>> waveFunction = new();
    private LootTable<WaveFunctionCollapseEntry> table = new();
    private Grid<string> grid;
    private Random random = new();

    public override void _Ready()
    {
        foreach (var entry in settings.Entries)
            table.AddItem(entry, entry.Weight);

        grid = new Grid<string>(settings.gridSize, Vector2I.Zero);
        InitializeWaveFunction();
        Generate();
    }

    private void InitializeWaveFunction()
    {
        waveFunction.Clear();
        for (int x = 0; x < settings.gridSize.X; x++)
        {
            for (int y = 0; y < settings.gridSize.Y; y++)
            {
                waveFunction[new Vector2I(x, y)] = settings.Entries.ToList();
            }
        }
    }

    private void Generate()
    {
        int iterations = 0;
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

        // Apply final state to grid
        foreach (var cell in waveFunction)
        {
            if (cell.Value.Count > 0)
            {
                // grid.Set(cell.Key, cell.Value[0].Scene.ResourcePath);
            }
        }
    }

    private void Collapse(Vector2I coords)
    {
        var entries = waveFunction[coords];
        if (entries.Count == 0) return;

        float totalWeight = entries.Sum(e => e.Weight);
        float rand = (float)(random.NextDouble() * totalWeight);

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

                waveFunction[otherCoords] = otherPossibleEntries
                    .Where(e => possibleNeighbors.Contains(e))
                    .ToList();

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

        if (!waveFunction.ContainsKey(neighborCoords)) return possibleNeighbors;

        foreach (var entry in waveFunction[coords])
        {
            var validNeighbors = directionKey switch
            {
                "Right" => entry.Right,
                "Left" => entry.Left,
                "Up" => entry.Up,
                "Down" => entry.Down,
                _ => Array.Empty<PackedScene>()
            };

            foreach (var otherEntry in waveFunction[neighborCoords])
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

            // Add small random noise to break ties
            var entropy = entries.Count + (float)(random.NextDouble() / 1000);
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