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
    private int minRooms = 4;

    [Export]
    private int maxRooms = 6;

    [Export]
    private int minRoomSize = 10;

    [Export]
    private int maxRoomSize = 30;

    [Export]
    private int tileSize = 32;

    [Export]
    private int roomSpawnRadius = 200;

    [Export]
    private float separationDistance = 1000f; // Distance threshold for separation

    [Export]
    private float separationStrength = 5.0f; // Force applied to move rooms apart

    [Export]
    private float damping = 0.9f; // Damping to reduce velocity over time

    [Node]
    private Node Rooms;

    [Node]
    private Camera2D camera;

    private bool apply;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        camera.Zoom = new Vector2(0.1f, 0.1f);

        CreateColliders();
    }

    public override void _Process(double delta)
    {
        if (!apply) return;

        ApplySeparationBehavior();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("interact")) apply = !apply;

        if (@event.IsActionPressed("open_inventory")) CreateRooms();

        if (!@event.IsActionPressed("ui_select")) return;

        Rooms.GetChildren().ToList().ForEach(room => room.QueueFree());
        CreateColliders();
    }

    private void CreateColliders()
    {
        var roomsCount = MathUtil.RNG.RandiRange(minRooms, maxRooms);

        for (var i = 0; i < roomsCount; i++)
        {
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
            Rooms.AddChild(collider);
        }
    }

    private void CreateRooms()
    {
        // var roomsCount = MathUtil.RNG.RandiRange(minRooms, maxRooms);
        //
        // for (var i = 0; i < roomsCount; i++)
        // {
        //     var size = new Vector2(
        //         MathUtil.RNG.RandiRange(minRoomSize, maxRoomSize),
        //         MathUtil.RNG.RandiRange(minRoomSize, maxRoomSize)
        //     ) * tileSize;
        //     var position = new Vector2(
        //         MathUtil.RNG.RandfRange(-roomSpawnRadius, roomSpawnRadius),
        //         MathUtil.RNG.RandfRange(-roomSpawnRadius, roomSpawnRadius)
        //     );
        //
        //     var room = Room.Create(position, size);
        //     room.SetMeta("size", size); // Store the size in metadata for future reference
        //     Rooms.AddChild(room);
        // }

        var rooms = Rooms.GetChildren<RigidBody2D>();
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
    }

    // TODO: Implement Separation Steering Behavior
    private void ApplySeparationBehavior()
    {
        if (Rooms.GetChildren().Any(child => child is not Room)) return;
        
        var rooms = Rooms.GetChildren<Room>();

        foreach (var roomA in rooms)
        {
            var separationVector = Vector2.Zero;

            foreach (var roomB in rooms.Where(r => r != roomA))
            {
                var distance = roomA.Position.DistanceTo(roomB.Position);

                if (distance > separationDistance) continue;

                // Calculate direction and strength of separation force
                var direction = (roomA.Position - roomB.Position).Normalized();
                var force = (separationDistance - distance) / separationDistance * separationStrength;

                // Apply weighted separation force based on distance
                separationVector += direction * force;
            }

            // Move roomA according to the accumulated separation vector
            roomA.Position += separationVector;
        }
    }
}