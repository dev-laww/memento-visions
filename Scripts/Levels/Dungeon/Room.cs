using Godot;
using GodotUtilities;

namespace Game.Levels.Dungeon;

[Scene]
public partial class Room : Node2D
{
    [Node]
    private ColorRect panel;

    private Rect2I bounds;

    public Rect2I Bounds => bounds;

    public Vector2I Center => bounds.Position + bounds.Size / 2;

    public bool Intersects(Room other, int padding = 0)
    {
        var thisXMin = bounds.Position.X - padding;
        var thisXMax = bounds.Position.X + bounds.Size.X + padding;
        var thisYMin = bounds.Position.Y - padding;
        var thisYMax = bounds.Position.Y + bounds.Size.Y + padding;

        var otherXMin = other.bounds.Position.X;
        var otherXMax = other.bounds.Position.X + other.bounds.Size.X;
        var otherYMin = other.bounds.Position.Y;
        var otherYMax = other.bounds.Position.Y + other.bounds.Size.Y;

        return !(thisXMax <= otherXMin || thisXMin >= otherXMax || thisYMax <= otherYMin || thisYMin >= otherYMax);
    }

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

        room.bounds = new Rect2I(position, size);

        return room;
    }
}