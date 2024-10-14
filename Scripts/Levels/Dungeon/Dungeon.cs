using System;
using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;
using Game.Utils.Generation;
using Game.Utils.Extensions;
using Godot;
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
    private Vector2I gridSize = new(60, 60);

    [Export]
    private int cellSize = 64;

    [Export]
    private int roomsCount;

    [Export]
    private Vector2I roomMaxSize = new(15, 15);

    [Node]
    private Node2D Map;

    [Node]
    private Node2D Rooms;

    [Node]
    private Node2D Hallways;

    private Grid<CellType> grid;
    private List<Room> rooms;
    private HashSet<IEdge> hallways;
    private bool pathFind;

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
        AddEntryPoints();
    }

    private void PlaceRooms()
    {
        var tries = 0;
        do
        {
            tries++;

            var location = new Vector2I().Random(Vector2I.Zero, gridSize);
            var roomSize = new Vector2I().Random(Vector2I.One * 7, roomMaxSize);
            var bounds = new Bounds(location, roomSize);
            var add = rooms.All(other => !other.Bounds.Intersects(bounds, padding: 2));

            // Check if the room is completely within the dungeon with padding
            if (bounds.Rect.xMin() < 0 || bounds.Rect.xMax() >= gridSize.X ||
                bounds.Rect.yMin() < 0 || bounds.Rect.yMax() >= gridSize.Y)
                add = false;

            if (!add) continue;

            var room = Room.Create(location * cellSize, roomSize * cellSize, bounds);
            Rooms.AddChild(room);
            rooms.Add(room);

            foreach (var pos in bounds.Rect.AllPositionsWithin())
                grid[pos] = CellType.Room;
        } while (rooms.Count < roomsCount && tries < 1000);
    }

    private void CreateHallways()
    {
        var edges = rooms.Select(r => r.Bounds.Center.ToVector()).Triangulate();
        var mst = edges.MinimumSpanningTree();

        mst.AddRange(edges.Where(edge => !mst.Contains(edge)).Where(_ => MathUtil.RNG.RandfRange(0, 1) <= 0.125f));

        hallways = new HashSet<IEdge>(mst);
    }

    private void AddEntryPoints()
    {
        foreach (var room in rooms)
        foreach (var entry in room.Bounds.EntryPoints)
        {
            var position = entry.Position;

            if (!IsWithinGridBounds(position)) continue;

            if (!HasHallwayNeighbor(position)|| !HasHallwayNeighbor(position + entry.Direction)) continue;

            entry.Toggle();
            room.Update();
        }
    }

    private bool HasHallwayNeighbor(Vector2I position)
    {
        var directions = new[]
        {
            Vector2I.Zero,
            Vector2I.Up,
            Vector2I.Down,
            Vector2I.Left,
            Vector2I.Right
        };

        return directions.Any(dir =>
        {
            for (var i = 1; i < 3; i++)
            {
                var pos = position + dir * i;

                if (!IsWithinGridBounds(pos)) return false;

                if (grid[pos] == CellType.Hallway) return true;
            }
            
            return false;
        });
    }

    private bool IsWithinGridBounds(Vector2I position)
    {
        return position.X >= 0 && position.X < gridSize.X &&
               position.Y >= 0 && position.Y < grid.Size.Y;
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
            }
        }
    }
}