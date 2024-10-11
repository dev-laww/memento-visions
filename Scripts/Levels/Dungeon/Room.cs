using Godot;
using GodotUtilities;

namespace Game.Levels.Dungeon;

[Scene]
public partial class Room : Node2D
{
    [Node]
    private ColorRect panel;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public static Room Create(Vector2I position, Vector2I size)
    {
        var room = GD.Load<PackedScene>("res://Scenes/Levels/Dungeon/Room.tscn").Instantiate<Room>();

        room.Position = position;
        room.panel.Size = size;

        return room;
    }
}