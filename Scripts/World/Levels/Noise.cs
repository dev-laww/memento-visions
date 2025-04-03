using System;
using System.Collections.Generic;
using System.Linq;
using Game.Autoload;
using Game.Common;
using Game.Components;
using Game.Data;
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
    private const float MIN_CHEST_DISTANCE = 500f;

    [Node] private TileMapLayer floor;
    [Node] private TileMapLayer middleGround;
    [Node] private TileMapLayer foreGround;
    [Node] private TileMapLayer props;
    [Node] private Node noiseGenerator;
    [Node] private Spawner spawner;
    [Node] private NavigationManager navigationManager;
    [Node] private Node2D entities;
    [Node] private ResourcePreloader resourcePreloader;
    [Node] private Node2D chests;
    [Node] private Timer showMarkersTimer;
    [Node] private Timer spawnBossTimer;

    [ExportToolButton("Generate", Icon = "RotateLeft")]
    private Callable Generate => Callable.From(() => noiseGenerator.Call("generate"));

    [ExportToolButton("Clear", Icon = "Clear")]
    private Callable Clear => Callable.From(() =>
    {
        noiseGenerator.Call("erase");
        entities.QueueFreeChildren();
        chests.QueueFreeChildren();
        navigationManager.Clear();
    });

    private GodotObject grid;
    private TextLoading loadingScreen;
    private Godot.Collections.Array<Vector2> validSpawnPositions;
    private bool bossSpawned;

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

        loadingScreen = new OverlayFactory.TextLoadingBuilder(GetTree())
            .SetText("Generating world...")
            .Build();

        Clear.Call();

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        Generate.Call();

        spawnBossTimer.Timeout += () =>
        {
            var text = new OverlayFactory.CenterTextBuilder(GetTree())
                .SetText("A boss is coming!")
                .SetDuration(5f)
                .Build();

            text.TreeExiting += SpawnBoss;
        };
        showMarkersTimer.Timeout += () => entities.GetChildrenOfType<Enemy>().ToList().ForEach(enemy =>
        {
            var marker = resourcePreloader.InstanceSceneOrNull<ScreenMarker>();
            marker.Offset = new Vector2(0, -32);
            marker.Target = enemy;
            marker.IsRed = true;

            enemy.AddChild(marker);
        });

        EnemyManager.EnemyCountChanged += count =>
        {
            if (count > 0 || !bossSpawned) return;

            var victoryScreen = resourcePreloader.InstanceSceneOrNull<Victory>();

            if (victoryScreen is null)
            {
                Log.Error("Failed to load victory screen");
                return;
            }

            GameManager.CurrentScene.AddChild(victoryScreen);
        };
    }

    private async void OnGenerationFinished()
    {
        MathUtil.RNG.Randomize();

        PurgeElevationTopEdges();
        GenerateSpawnPosition();
        SpawnChests();
        SpawnPlayer();
        SpawnEnemies();

        if (Engine.IsEditorHint())
        {
            SpawnBoss();
        }

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
        Clear.Call();
    }

    private void GenerateSpawnPosition()
    {
        if (loadingScreen is not null && !Engine.IsEditorHint())
        {
            loadingScreen.Text = "Generating spawn points...";
        }

        var layerCount = grid.Call("get_layer_count").AsInt32();
        var emptyCells = GetEmptyCells();

        do
        {
            layerCount--;

            var cells = GetEmptyCells(layerCount);
            emptyCells = emptyCells.Intersect(cells);
        } while (layerCount > 0);

        var tileSize = floor.TileSet.TileSize;

        validSpawnPositions = [.. emptyCells.Select(cell => cell * tileSize + tileSize / 2)];
    }

    private void SpawnPlayer()
    {
        if (loadingScreen is not null && !Engine.IsEditorHint())
        {
            loadingScreen.Text = "Spawning player...";
        }

        var player = resourcePreloader.InstanceSceneOrNull<Player>();
        var spawnPoint = validSpawnPositions.PickRandom();
        validSpawnPositions.Remove(spawnPoint);

        player.Position = spawnPoint;

        entities.AddChild(player);

        if (!Engine.IsEditorHint()) return;

        player.SetOwner(GetTree().GetEditedSceneRoot());
    }

    private void SpawnChests()
    {
        var chestsCount = MathUtil.RNG.RandiRange(10, 20);
        var pickedChestPositions = new HashSet<Vector2>();

        for (var i = 0; i < chestsCount; i++)
        {
            var chest = resourcePreloader.InstanceSceneOrNull<Chest>();
            // TODO: get random loot resource
            // TODO: prevent spawning chests beside each other
            var position = validSpawnPositions.PickRandom();

            pickedChestPositions.Add(position);
            validSpawnPositions.Remove(position);

            chest.Position = position;
            chest.SetDrops(LootTableRegistry.GetRandom());

            chests.AddChild(chest);
        }
    }


    private void SpawnEnemies()
    {
        spawner.SetSpawnPositions(validSpawnPositions);

        if (loadingScreen is not null && !Engine.IsEditorHint())
        {
            loadingScreen.Text = "Spawning enemies...";
        }

        spawner.Spawn();
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

    private void SpawnBoss()
    {
        var playerPosition = this.GetPlayer().Position + MathUtil.RNG.RandDirection() * 50;

        spawner.SpawnBoss(playerPosition);

        if (Engine.IsEditorHint()) return;

        GameCamera.Shake(1f);
        bossSpawned = true;
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