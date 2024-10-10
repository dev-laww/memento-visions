using System;
using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;
using Game.Utils.Dungeon;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.Levels.Dungeon;

// TODO: Enlarge rooms
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
    private Vector2I size = new(30, 30);

    [Export]
    private int roomsCount;

    [Export]
    private Vector2I roomMaxSize = new(10, 10);

    [Node]
    private Node Rooms;

    private Grid<CellType> grid;
    private List<Room> rooms;
    private HashSet<IEdge> hallways;
    private bool pathFind;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready() => Generate();

    public override void _Process(double delta) => QueueRedraw();

    public override void _Draw()
    {
        DrawRect(new Rect2(Vector2.Zero, size), Colors.Red, filled: false);
        //
        // foreach (var edge in hallways)
        //     DrawLine(edge.P.ToVector(), edge.Q.ToVector(), Colors.Blue);

        if (pathFind)
            PathFindHallways();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("interact"))
            pathFind = !pathFind;


        if (!@event.IsActionPressed("ui_accept")) return;
        Rooms.GetChildren().ToList().ForEach(c => c.QueueFree());
        Generate();
    }

    private void Generate()
    {
        // Scale the size for the grid creation
        grid = new Grid<CellType>(size, Vector2I.Zero);
        rooms = new List<Room>();

        PlaceRooms();
        CreateHallways();
    }

    private void PlaceRooms()
    {
        var tries = 0;
        do
        {
            tries++;

            var location = new Vector2I().Random(Vector2I.Zero, size);
            var roomSize = new Vector2I().Random(Vector2I.One, roomMaxSize);
            var room = Room.Create(location, roomSize);
            var add = rooms.All(other => !other.Intersects(room, padding: 1));

            // Check if the room is completely within the dungeon with padding
            if (room.Bounds.xMin() < 1 || room.Bounds.xMax() >= size.X - 1 ||
                room.Bounds.yMin() < 1 || room.Bounds.yMax() >= size.Y - 1)
                add = false;

            if (!add)
            {
                room.QueueFree();
                continue;
            }

            rooms.Add(room);
            Rooms.AddChild(room);

            foreach (var pos in room.Bounds.AllPositionsWithin())
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
        var pathFinder = new PathFinder(size);

        foreach (
            var path in
            from edge in hallways
            let start = edge.P.ToVectorI()
            let end = edge.Q.ToVectorI()
            select pathFinder.FindPath(start, end, (PathFinder.Node a, PathFinder.Node b) =>
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
                DrawRect(new Rect2(pos, Vector2.One), Colors.Blue);
            }
        }
    }
}