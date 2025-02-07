using Game.Generation.Dungeon;
using Godot;
using GodotUtilities;

namespace Game.Levels;

[Tool]
[Scene]
public partial class Room : Node2D
{
    [Node] private Label Label;

    public Bounds Bounds { get; private set; }

    public Vector2I Size { get; private set; }

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public static Room Create(Vector2I position, Vector2I size, Bounds bounds)
    {
        var room = GD.Load<PackedScene>("res://Scenes/Levels/Dungeon/Room.tscn").Instantiate<Room>();

        room.Position = position;
        room.Bounds = bounds;
        room.Size = size;
        room.Label.Position = size / 2;

        return room;
    }
}