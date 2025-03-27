using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Game.Common.Extensions;
using Game.Components;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game;

[Tool]
[Scene]
public partial class Noise : Node2D
{
    [Node] private TileMapLayer floor;
    [Node] private TileMapLayer middleGround;
    [Node] private TileMapLayer foreGround;
    [Node] private TileMapLayer props;
    [Node] private Node noiseGenerator;
    [Node] private Spawner spawner;
    [Node] private NavigationManager navigationManager;

    private GodotObject grid;
    private Stopwatch stopwatch = new();
    private bool placedFirstProp;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        grid = noiseGenerator.Get("grid").AsGodotObject();
        noiseGenerator.Connect("generation_finished", Callable.From(OnGenerationFinished));
    }


    private void OnGenerationFinished()
    {
        stopwatch.Restart();
        GD.Print("Terrain generated!");

        GD.Print("Cleaning up...");
        PurgeElevationTopEdges();
        stopwatch.Stop();
        GD.Print($"Elapsed time: {stopwatch.ElapsedMilliseconds}ms");

        GD.Print("Generating spawn points...");
        stopwatch.Restart();
        GenerateSpawnPosition();
        stopwatch.Stop();
        GD.Print($"Elapsed time: {stopwatch.ElapsedMilliseconds}ms");

        GD.Print("Spawning enemies...");
        stopwatch.Restart();
        spawner.StartSpawning();
        stopwatch.Stop();
        GD.Print($"Elapsed time: {stopwatch.ElapsedMilliseconds}ms");

        GD.Print("Placing navigation regions...");
        GetTree().CreateTimer(0.2f).Timeout += () =>
        {
            stopwatch.Restart();
            navigationManager.PlaceNavigationRegions();
            stopwatch.Stop();
        };
        GD.Print($"Elapsed time: {stopwatch.ElapsedMilliseconds}ms");
    }

    private void GenerateSpawnPosition()
    {
        var layerCount = grid.Call("get_layer_count").AsInt32();
        var emptyCells = GetEmptyCells();

        do
        {
            layerCount--;

            var cells = GetEmptyCells(layerCount);
            emptyCells = emptyCells.Intersect(cells);

        } while (layerCount > 0);

        var tileSize = floor.TileSet.TileSize;

        foreach (var cell in emptyCells)
        {
            var position = (cell * tileSize) + tileSize / 2;

            spawner.SpawnPoints.Add(position);
        }
    }

    private void PurgeElevationTopEdges()
    {
        var tileSize = floor.TileSet.TileSize;
        var occupiedCells = GetOccupiedCells(layer: 1);
        var clusters = GetClusters([.. occupiedCells]);
        var innerCells = clusters.SelectMany(GetInnerCells);

        var topEdges = clusters.SelectMany(cluster => GetEdges(cluster, Vector2I.Up));
        var topEdgesSet = new HashSet<Vector2I>(topEdges);
        var innerTopEdges = new HashSet<Vector2I>();

        foreach (var cell in topEdgesSet)
        {
            var left = new Vector2I(cell.X - 1, cell.Y);
            var right = new Vector2I(cell.X + 1, cell.Y);
            var topLeft = new Vector2I(cell.X - 1, cell.Y - 1);
            var topRight = new Vector2I(cell.X + 1, cell.Y - 1);

            var cornerCondition = occupiedCells.Contains(left) && occupiedCells.Contains(right) &&
                                   !occupiedCells.Contains(topLeft) && !occupiedCells.Contains(topRight);

            var contiguousCondition = topEdgesSet.Contains(left) || topEdgesSet.Contains(right);

            if (cornerCondition || contiguousCondition)
            {
                innerTopEdges.Add(cell);
            }
        }

        foreach (var cell in innerTopEdges)
        {
            var marker = new Marker2D
            {
                Position = (cell * tileSize) + tileSize / 2,
            };

            for (var i = 0; i < MathUtil.RNG.RandiRange(0, 3); i++)
            {
                var cellToErase = new Vector2I(cell.X + MathUtil.RNG.RandSign(), cell.Y);
                var markerToErase = new Marker2D
                {
                    Position = (cellToErase * tileSize) + tileSize / 2,
                };

                GetTree().CreateTimer(0.1f).Timeout += () => foreGround.EraseCell(cellToErase);
            }

            GetTree().CreateTimer(0.1f).Timeout += () => foreGround.EraseCell(cell);
        }
    }

    private static List<HashSet<Vector2I>> GetClusters(HashSet<Vector2I> occupiedCells)
    {
        var clusters = new List<HashSet<Vector2I>>();
        var visited = new HashSet<Vector2I>();

        foreach (var cell in occupiedCells)
        {
            if (visited.Contains(cell))
                continue;

            var cluster = new HashSet<Vector2I>();
            var stack = new Stack<Vector2I>();
            stack.Push(cell);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (visited.Contains(current))
                    continue;

                visited.Add(current);
                cluster.Add(current);

                foreach (var neighbor in current.GetNeighbors())
                {
                    if (occupiedCells.Contains(neighbor) && !visited.Contains(neighbor))
                    {
                        stack.Push(neighbor);
                    }
                }
            }

            clusters.Add(cluster);
        }

        return clusters;
    }

    private static IEnumerable<Vector2I> GetInnerCells(HashSet<Vector2I> cluster)
    {
        foreach (var cell in cluster)
        {
            if (!cell.GetNeighbors().All(cluster.Contains)) continue;

            yield return cell;
        }
    }

    private static IEnumerable<Vector2I> GetEdges(HashSet<Vector2I> cluster, Vector2I direction)
    {
        foreach (var cell in cluster)
        {
            var neighborToCheck = cell + direction;

            if (cluster.Contains(neighborToCheck)) continue;

            yield return cell;
        }
    }

    private IEnumerable<Vector2I> GetEmptyCells(int layer = 0)
    {
        var worldSize = noiseGenerator.Get("settings").AsGodotObject().Get("world_size").AsVector2();

        for (var x = 0; x < worldSize.X; x++)
        {
            for (var y = 0; y < worldSize.Y; y++)
            {
                var cell = new Vector2I(x, y);
                var value = grid.Call("get_value", cell, layer).As<Resource>();

                if (value is not null) continue;

                yield return cell;
            }
        }
    }

    private IEnumerable<Vector2I> GetOccupiedCells(int layer = 0)
    {
        var worldSize = noiseGenerator.Get("settings").AsGodotObject().Get("world_size").AsVector2();

        for (var x = 0; x < worldSize.X; x++)
        {
            for (var y = 0; y < worldSize.Y; y++)
            {
                var cell = new Vector2I(x, y);
                var value = grid.Call("get_value", cell, layer).As<Resource>();

                if (value is null) continue;

                yield return cell;
            }
        }
    }
}
