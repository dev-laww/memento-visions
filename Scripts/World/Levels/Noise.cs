using System.Collections.Generic;
using System.Linq;
using Game.Common.Extensions;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;
using Priority_Queue;

namespace Game;

[Tool]
[Scene]
public partial class Noise : Node2D
{
    [Node] private TileMapLayer floor;
    [Node] private TileMapLayer middleGround;
    [Node] private TileMapLayer foreGround;
    [Node] private TileMapLayer props;
    [Node] private TileMapLayer test;
    [Node] private Node noiseGenerator;

    private GodotObject grid;

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
        test.Clear();
        this.GetAllChildrenOfType<Marker2D>().ToList().ForEach(marker => marker.QueueFree());

        // fill empty cells with a tile
        var layerCount = grid.Call("get_layer_count").AsInt32();
        var emptyCells = GetEmptyCells();

        do
        {
            layerCount--;

            var cells = GetEmptyCells(layerCount);
            emptyCells = emptyCells.Intersect(cells);

        } while (layerCount > 0);


        var occupiedCells = GetOccupiedCells(layer: 1);
        var clusters = GetClusters([.. occupiedCells]);
        var innerCells = clusters.SelectMany(GetInnerCells);

        var spawnableCells = innerCells.Union(emptyCells).ToList();

        foreach (var cell in spawnableCells)
        {
            var marker = new Marker2D
            {
                Position = (cell * 48) + new Vector2(24, 24)
            };

            this.EditorAddChild(marker);
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
