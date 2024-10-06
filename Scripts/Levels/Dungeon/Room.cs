using Godot;
using GodotUtilities;

namespace Game;

[Scene]
public partial class Room : Node2D
{
    [Node]
    private Panel panel;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public static Room Create(Vector2 position, Vector2 size)
    {
        var room = GD.Load<PackedScene>("res://Scenes/Levels/Dungeon/Room.tscn").Instantiate<Room>();

        room.Position = position;
        room.panel.Size = size;
        room.panel.Position = -size / 2;

        return room;
    }

    public static RigidBody2D CreateCollider(Vector2 position, Vector2 size)
    {
        var collider = new RigidBody2D
        {
            CollisionLayer = 20,
            CollisionMask = 20,
            LockRotation = true,
            Freeze = true,
        };

        var shape = new RectangleShape2D { Size = size };
        var collision = new CollisionShape2D { Shape = shape };
        collider.AddChild(collision);
        collider.SetMeta("size", size);
        collider.Position = position;

        return collider;
    }
}