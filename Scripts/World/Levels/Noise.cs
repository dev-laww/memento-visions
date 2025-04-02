using System.Collections.Generic;
using System.Linq;
using Game.Components;
using Game.Entities;
using Game.UI.Screens;
using Game.Utils;
using Game.Utils.Extensions;
using Game.World;
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
    [Node] private Node2D entities;
    [Node] private ResourcePreloader resourcePreloader;

    [ExportToolButton("Generate", Icon = "RotateLeft")]
    private Callable Generate => Callable.From(() =>
    {
        GD.Print("Generating noise...");
        noiseGenerator.Call("generate");
    });

    [ExportToolButton("Clear", Icon = "Clear")]
    private Callable Clear => Callable.From(() =>
    {
        GD.Print("Clearing noise...");
        noiseGenerator.Call("erase");
        entities.QueueFreeChildren();
        navigationManager.Clear();
    });

    private GodotObject grid;
    private bool placedFirstProp;
    private TextLoading loadingScreen;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override async void _Ready()
    {
        grid = noiseGenerator.Get("grid").AsGodotObject();
        noiseGenerator.Connect("generation_finished", Callable.From(OnGenerationFinished));
        noiseGenerator.Connect("generation_started", Callable.From(OnGenerationStarted));

        if (Engine.IsEditorHint()) return;

        loadingScreen = new LoadingScreenFactory.TextLoadingBuilder(GetTree())
            .SetText("Generating world...")
            .Build();

        Clear.Call();

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        Generate.Call();
    }


    private async void OnGenerationFinished()
    {
        PurgeElevationTopEdges();
        GenerateSpawnPosition();
        SpawnChests();
        SpawnPlayer();
        SpawnEnemies();

        await ToSignal(GetTree().CreateTimer(0.2f), "timeout");

        if (loadingScreen is not null && !Engine.IsEditorHint())
        {
            loadingScreen.Text = "Placing navigation regions...";
        }

        navigationManager.PlaceNavigationRegions();

        await ToSignal(GetTree().CreateTimer(0.2f), "timeout");

        loadingScreen?.QueueFree();
        loadingScreen = null;
    }

    private void OnGenerationStarted()
    {
        entities.QueueFreeChildren();
    }

    private void GenerateSpawnPosition()
    {
        if (loadingScreen is not null && !Engine.IsEditorHint())
        {
            loadingScreen.Text = "Generating spawn points...";
        }

        var layerCount = grid.Call("get_layer_count").AsInt32();
        var emptyCells = GetEmptyCells();
        spawner.SpawnPoints.Clear();

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

    private void SpawnPlayer()
    {
        if (loadingScreen is not null && !Engine.IsEditorHint())
        {
            loadingScreen.Text = "Spawning player...";
        }

        var player = resourcePreloader.InstanceSceneOrNull<Player>();
        var spawnPoint = spawner.SpawnPoints.PickRandom();
        spawner.SpawnPoints.Remove(spawnPoint);

        player.Position = spawnPoint;

        entities.AddChild(player);

        if (!Engine.IsEditorHint()) return;

        player.SetOwner(GetTree().GetEditedSceneRoot());
    }

    private void SpawnChests()
    {
        var chestsCount = MathUtil.RNG.RandiRange(10, 20);

        for (var i = 0; i < chestsCount; i++)
        {
            var chest = resourcePreloader.InstanceSceneOrNull<Chest>();
            // TODO: get random loot resource
            var position = spawner.SpawnPoints.PickRandom();
            spawner.SpawnPoints.Remove(position);

            chest.Position = position;

            entities.AddChild(chest);

            if (!Engine.IsEditorHint()) return;

            chest.SetOwner(GetTree().GetEditedSceneRoot());
        }
    }


    private void SpawnEnemies()
    {
        if (loadingScreen is not null && !Engine.IsEditorHint())
        {
            loadingScreen.Text = "Spawning enemies...";
        }

        var spawnedEnemies = spawner.Spawn();

        foreach (var enemy in spawnedEnemies)
        {
            entities.AddChild(enemy);

            if (!Engine.IsEditorHint()) continue;

            enemy.SetOwner(GetTree().GetEditedSceneRoot());
        }
    }

    private void PurgeElevationTopEdges()
    {
        if (loadingScreen is not null && !Engine.IsEditorHint())
        {
            loadingScreen.Text = "Terrain generated!";
            loadingScreen.Text = "Cleaning up...";
        }

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