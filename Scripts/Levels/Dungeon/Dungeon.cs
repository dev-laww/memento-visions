using System;
using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;
using Game.Utils;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game;

[Scene]
public partial class Dungeon : Node2D
{
    [Export]
    private int minRooms = 6;

    [Export]
    private int maxRooms = 10;

    [Export]
    private int minRoomSize = 10;

    [Export]
    private int maxRoomSize = 30;

    [Export]
    private int tileSize = 32;

    [Export]
    private int roomSpawnRadius = 200;

    [Export]
    private float separationStrength = 0.05f; // Force applied to move rooms apart

    [Export]
    private float damping = 1.2f; // Damping to reduce velocity over time

    [Export]
    private float separationDivisor = 0.4f; // Divisor to reduce separation distance

    [Node]
    private Node Rooms;

    [Node]
    private Camera2D camera;

    [Node]
    private Timer timer;

    [Signal]
    public delegate void InvalidRoomsEventHandler();

    [Signal]
    public delegate void SettledEventHandler();

    private bool settled;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        camera.Zoom = new Vector2(0.1f, 0.1f);

        CreateColliders();
        InvalidRooms += CreateColliders;
        timer.Timeout += CreateRooms;
        Settled += QueueRedraw;
    }

    public override void _PhysicsProcess(double delta) => ApplySeparationBehavior();

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("open_inventory")) CreateRooms();

        if (@event.IsActionPressed("interact")) QueueRedraw();

        if (!@event.IsActionPressed("ui_select")) return;

        settled = false;
        QueueRedraw();
        Rooms.GetChildren().ToList().ForEach(room => room.QueueFree());
        CreateColliders();
    }

    private void CreateColliders()
    {
        var roomsCount = MathUtil.RNG.RandiRange(minRooms, maxRooms);
        var points = Generator.GeneratePoints(roomsCount, roomSpawnRadius, tileSize);

        points.ForEach(point =>
        {
            var size = new Vector2(
                MathUtil.RNG.RandiRange(minRoomSize, maxRoomSize),
                MathUtil.RNG.RandiRange(minRoomSize, maxRoomSize)
            ) * tileSize;

            var collider = Room.CreateCollider(point, size);

            Rooms.AddChild(collider);
        });

        timer.Start();
    }


    private void CreateRooms()
    {
        var rooms = Rooms.GetChildren<RigidBody2D>();

        if (!RoomsValid())
        {
            Rooms.GetChildren().ToList().ForEach(room => room.QueueFree());
            CreateColliders();
            return;
        }

        var positions = new List<Tuple<Vector2, Vector2>>();

        foreach (var room in rooms)
        {
            var size = (Vector2)room.GetMeta("size");
            positions.Add(new Tuple<Vector2, Vector2>(room.Position, size));
            room.QueueFree();
        }

        foreach (var position in positions)
        {
            var room = Room.Create(position.Item1, position.Item2);
            room.SetMeta("size", position.Item2); // Store the size in metadata for future reference
            Rooms.AddChild(room);
        }

        timer.Stop();
    }

    private bool RoomsValid()
    {
        var rooms = Rooms.GetChildren<RigidBody2D>();

        var areas = (
            from room in rooms
            let size = (Vector2)room.GetMeta("size")
            select new Rect2(room.Position - size / 2, size)
        ).ToList();

        if (!areas.Any(area => areas.Where(other => area != other).Any(other => area.Intersects(other))))
            return true;

        separationDivisor += 0.01f;
        EmitSignal(SignalName.InvalidRooms);
        return false;
    }

    // TODO: Detect when rooms stop moving
    private void ApplySeparationBehavior()
    {
        if (settled) return;

        var rooms = Rooms.GetChildren<Node2D>();

        if (rooms.All(room => room is Room))
        {
            EmitSignal(SignalName.Settled);
            settled = true;
            return;
        }

        // Apply marginally better separation behavior
        foreach (var room in rooms)
        {
            var separation = Vector2.Zero;

            foreach (var other in rooms)
            {
                if (room == other) continue;

                var distance = room.Position.DistanceTo(other.Position);
                var desiredSeparation = (
                    (Vector2)room.GetMeta("size") + (Vector2)other.GetMeta("size")
                ).Length() * separationDivisor;

                if (distance > desiredSeparation) continue;

                var direction = (room.Position - other.Position).Normalized();
                var force = (desiredSeparation - distance) * separationStrength;
                separation += direction * force;
            }

            separation *= damping;
            room.Position += separation;
            room.Position = room.Position.SnapToGrid();
        }
    }

    public override void _Draw()
    {
        if (!settled) return;

        var corridors = CreateCorridors();

        foreach (var edge in corridors)
        {
            var (p1, p2) = (edge.P.ToVector(), edge.Q.ToVector());

            DrawLine(p1, new Vector2(p1.X, p2.Y), Colors.Red);
            DrawLine(new Vector2(p1.X, p2.Y), p2, Colors.Red);
        }
    }

    // TODO: Implement a better algorithm to create corridors, possibly using A* pathfinding or hybridizing with Delaunay Triangulation
    private List<IEdge> CreateCorridors()
    {
        var rooms = Rooms.GetChildren<Node2D>();
        var edges = rooms.Select(room => room.Position).Triangulate();
        var mst = edges.MinimumSpanningTree();

        mst.AddRange(edges.Where(edge => !mst.Contains(edge)).Where(_ => MathUtil.RNG.RandfRange(0, 1) <= 0.2f));

        return mst;
    }
}