using System;
using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;
using Game.Generation.Dungeon;
using Game.Extensions;
using Godot;
using Godot.Collections;
using GodotUtilities;

namespace Game.Levels.Dungeon;

// TODO: Add doors
// TODO: Add enemy spawn system
// TODO: Multilevel dungeons for roguelike experience
[Scene]
public partial class Dungeon : Node2D
{
    private enum CellType
    {
        None,
        Room,
        Hallway
    }

    [Export]
    private Vector2I gridSize = new(40, 40);

    [Export]
    private int cellSize = 128;

    [Export]
    private int roomsCount;

    [Export]
    private Vector2I roomMaxSize = new(10, 10);

    [Node]
    private Node2D Map;

    [Node]
    private Node2D Rooms;

    [Node]
    private Node2D Hallways;

    [Node]
    private TileMapLayer TileMap;

    [Node]
    private TileMapLayer Ground;

    private Grid<CellType> grid;
    private List<Room> rooms;
    private HashSet<IEdge> hallways;
    private bool pathFind;
    private Array<Vector2I> terrain = new();

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        Generate();
        Map.Position = new Vector2(-gridSize.X * cellSize / 2f, -gridSize.Y * cellSize / 2f);
    }

    public override void _Process(double delta) => QueueRedraw();

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("interact"))
        {
            pathFind = !pathFind;
            if (!pathFind)
            {
                Hallways.GetChildren().ToList().ForEach(c => c.QueueFree());
                return;
            }

            PathFindHallways();
        }


        if (!@event.IsActionPressed("ui_accept")) return;
        TileMap.Clear();
        terrain.Clear();
        Ground.Clear();
        Rooms.GetChildren().ToList().ForEach(c => c.QueueFree());
        Generate();
    }

    private void Generate()
    {
        // Scale the gridSize for the grid creation
        grid = new Grid<CellType>(gridSize, Vector2I.Zero);
        rooms = new List<Room>();

        PlaceRooms();
        CreateHallways();
        PathFindHallways();
        TileMap.SetCellsTerrainConnect(terrain, 0, 0);
        for (var i = 0; i < gridSize.X; i++)
        for (var j = 0; j < gridSize.Y; j++)
        {
            var cell = new Vector2I(i, j);

            Ground.SetCell(cell, atlasCoords: new Vector2I(0, 4), sourceId: 4);


            if (grid[i, j] != CellType.None) continue;

            var atlas = new Vector2I(0, 0);

            TileMap.SetCell(cell, atlasCoords: atlas, sourceId: 4);
        }
    }

    private void PlaceRooms()
    {
        var tries = 0;
        do
        {
            tries++;

            var location = new Vector2I().Random(Vector2I.Zero, gridSize);
            var roomSize = new Vector2I().Random(Vector2I.One * 10, roomMaxSize);

            // Calculate the center and radius for an oval (or circle if width and height are equal)
            var centerX = roomSize.X / 2f;
            var centerY = roomSize.Y / 2f;

            var bounds = new Bounds(location, roomSize);
            var add = rooms.All(other => !other.Bounds.Intersects(bounds, padding: 1));

            // Check if the room is completely within the dungeon with padding
            if (bounds.Rect.xMin() < 0 || bounds.Rect.xMax() >= gridSize.X ||
                bounds.Rect.yMin() < 0 || bounds.Rect.yMax() >= gridSize.Y)
                add = false;

            if (!add) continue;

            // Only add cells that fall within the elliptical shape
            var room = Room.Create(location * cellSize, roomSize * cellSize, bounds);
            foreach (var pos in bounds.Rect.AllPositionsWithin())
            {
                var localPos = pos - location;
                var dx = (localPos.X - centerX) / centerX;
                var dy = (localPos.Y - centerY) / centerY;

                // Check if within ellipse (circular if centerX == centerY)
                if (!(dx * dx + dy * dy <= 1)) continue;

                grid[pos] = CellType.Room;
                terrain.Add(pos);
            }

            Rooms.AddChild(room);
            rooms.Add(room);
        } while (rooms.Count < roomsCount && tries < 1000);
    }

    private void CreateHallways()
    {
        var edges = rooms.Select(r => r.Bounds.Center.ToVector()).Triangulate();
        var mst = edges.MinimumSpanningTree();

        mst.AddRange(edges.Where(edge => !mst.Contains(edge)).Where(_ => MathUtil.RNG.RandfRange(0, 1) <= 0.125f));

        hallways = new HashSet<IEdge>(mst);
    }

    private void PathFindHallways()
    {
        Hallways.GetChildren().ToList().ForEach(c => c.QueueFree());
        var pathFinder = new PathFinder(gridSize);

        foreach (
            var path in
            from edge in hallways
            let start = edge.P.ToVectorI()
            let end = edge.Q.ToVectorI()
            select pathFinder.FindPath(start, end, (a, b) =>
            {
                var pathCost = new PathFinder.PathCost
                {
                    Cost = b.Position.ToVector().ManhattanDistanceTo(end)
                };

                pathCost.Cost += grid[b.Position] switch
                {
                    CellType.None => 5,
                    CellType.Hallway => 1,
                    CellType.Room => 10000,
                    _ => throw new ArgumentOutOfRangeException()
                };

                pathCost.Traversable = true;

                return pathCost;
            })
        )
        {
            if (path == null) return;

            foreach (var current in path.Where(current => grid[current] == CellType.None))
                grid[current] = CellType.Hallway;

            foreach (var pos in path.Where(pos => grid[pos] == CellType.Hallway))
            {
                var hallway = Hallway.Create(pos * cellSize);

                Hallways.AddChild(hallway);
                terrain.Add(pos);
            }
        }
    }
}