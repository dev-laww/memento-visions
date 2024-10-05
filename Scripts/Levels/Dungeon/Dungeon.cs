using System;
using System.Collections.Generic;
using System.Linq;
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
    }

    public override void _Process(double _delta) => ApplySeparationBehavior();

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("open_inventory")) CreateRooms();

        if (!@event.IsActionPressed("ui_select")) return;

        Rooms.GetChildren().ToList().ForEach(room => room.QueueFree());
        CreateColliders();
    }

    private void CreateColliders()
    {
        var roomsCount = MathUtil.RNG.RandiRange(minRooms, maxRooms);
        var usedPositions = new HashSet<Vector2>();

        for (var i = 0; i < roomsCount; i++)
        {
            Vector2 position;

            do
            {
                position = new Vector2(
                    MathUtil.RNG.RandfRange(-roomSpawnRadius, roomSpawnRadius),
                    MathUtil.RNG.RandfRange(-roomSpawnRadius, roomSpawnRadius)
                );

                // Introduce randomness by adding an offset
                var offsetX = MathUtil.RNG.RandfRange(-tileSize, tileSize);
                var offsetY = MathUtil.RNG.RandfRange(-tileSize, tileSize);
                position += new Vector2(offsetX, offsetY);
            } while (IsPositionOccupied(position, usedPositions));

            usedPositions.Add(position);

            var collider = new RigidBody2D
            {
                GravityScale = 0,
                CollisionLayer = 20,
                CollisionMask = 20,
                LockRotation = true,
            };
            var size = new Vector2(
                MathUtil.RNG.RandiRange(minRoomSize, maxRoomSize),
                MathUtil.RNG.RandiRange(minRoomSize, maxRoomSize)
            ) * tileSize;

            var shape = new RectangleShape2D { Size = size };
            var collision = new CollisionShape2D { Shape = shape };
            collider.AddChild(collision);
            collider.SetMeta("size", size);
            collider.Position = position; // Set the position here
            Rooms.AddChild(collider);
        }

        timer.Start();
    }

    private bool IsPositionOccupied(Vector2 position, HashSet<Vector2> usedPositions)
    {
        const float proximityThreshold = 50f; // Adjust as needed for proximity checks

        // Check if the new position is too close to any used position
        return usedPositions.Any(usedPos => position.DistanceTo(usedPos) < proximityThreshold);
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

    private void ApplySeparationBehavior()
    {
        var rooms = Rooms.GetChildren<Node2D>();

        // Apply marginally better separation behavior
        foreach (var room in rooms)
        {
            var separation = Vector2.Zero;

            foreach (var other in rooms)
            {
                if (room == other) continue;

                var distance = room.Position.DistanceTo(other.Position);
                var desiredSeparation = ((Vector2)room.GetMeta("size") + (Vector2)other.GetMeta("size")).Length() *
                                        separationDivisor;

                if (distance > desiredSeparation) continue;

                var direction = (room.Position - other.Position).Normalized();
                var force = (desiredSeparation - distance) * separationStrength;
                separation += direction * force;
            }

            separation *= damping;
            room.Position += separation;
        }
    }
}