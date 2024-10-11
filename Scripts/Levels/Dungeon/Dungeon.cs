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
    private int cellSize = 128;

    [Export]
    private int roomsCount;

    [Export]
    private Vector2I roomMaxSize = new(15, 15);

    [Node]
    private Node2D Rooms;

    private Grid<CellType> grid;
    private List<Bounds> rooms;
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
        Rooms.Position = new Vector2(-gridSize.X * cellSize / 2f, -gridSize.Y * cellSize / 2f);
    }

    public override void _Process(double delta) => QueueRedraw();

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("interact"))
        {
            pathFind = !pathFind;
            if (!pathFind)
                Rooms.GetChildren().OfType<ColorRect>().ToList().ForEach(c => c.QueueFree());

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
        rooms = new List<Bounds>();

        PlaceRooms();
        CreateHallways();
        PathFindHallways();
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
            var add = rooms.All(other => !other.Intersects(bounds, padding: 1));

            // Check if the room is completely within the dungeon with padding
            if (bounds.Rect.xMin() < 0 || bounds.Rect.xMax() >= gridSize.X ||
                bounds.Rect.yMin() < 0 || bounds.Rect.yMax() >= gridSize.Y)
                add = false;

            if (!add) continue;

            rooms.Add(bounds);
            var room = Room.Create(location * cellSize, roomSize * cellSize);
            Rooms.AddChild(room);

            foreach (var pos in bounds.Rect.AllPositionsWithin())
                grid[pos] = CellType.Room;
        } while (rooms.Count < roomsCount && tries < 1000);
    }

    private void CreateHallways()
    {
        var edges = rooms.Select(r => r.Center.ToVector()).Triangulate();
        var mst = edges.MinimumSpanningTree();

        mst.AddRange(edges.Where(edge => !mst.Contains(edge)).Where(_ => MathUtil.RNG.RandfRange(0, 1) <= 0.125f));

        hallways = new HashSet<IEdge>(mst);
    }

    private void PathFindHallways()
    {
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
                    Cost = b.Position.DistanceTo(end)
                };

                if (grid[b.Position] != CellType.Room)
                {
                    switch (grid[b.Position])
                    {
                        case CellType.None:
                            pathCost.Cost += 5;
                            break;
                        case CellType.Hallway:
                            pathCost.Cost += 1;
                            break;
                        case CellType.Room:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    pathCost.Cost += 10;
                }

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
                var hallway = new ColorRect
                {
                    Position = pos * cellSize,
                    Size = Vector2.One * cellSize,
                    Visible = pathFind,
                    Color = Colors.Blue
                };

                Rooms.AddChild(hallway);
            }
        }
    }
}