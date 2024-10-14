using System.Linq;
using Game.Utils.Generation;
using Godot;
using GodotUtilities;

namespace Game.Levels.Dungeon;

[Scene]
public partial class Room : Node2D
{
    [Node]
    private ColorRect Background;

    [Node]
    private Label Label;

    public Bounds Bounds { get; private set; }

    public Vector2I Size { get; private set; }

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public void Update()
    {
        var entryPointsAsStrings =
            Bounds.EntryPoints.Where(e => e.Open).Select(point => point.Direction.ToString()).ToArray();
        Label.Text =
            $"Position: {Bounds.Rect.Position}\nSize: {Bounds.Rect.Size}\nEntry Points: {string.Join("\n ", entryPointsAsStrings)}";

        var entry = new ColorRect
        {
            Color = Colors.Red,
            Size = new Vector2(64, 64),
        };

        foreach (var point in Bounds.EntryPoints.Where(point => point.Open))
        {
            if (entry.Duplicate() is not ColorRect entryPoint) continue;
            
            var halfX = Size.X / 2f;
            var halfY = Size.Y / 2f;
            var subtractX = Bounds.Rect.Size.X % 2 == 0 ? 0 : 32;
            var subtractY = Bounds.Rect.Size.Y % 2 == 0 ? 0 : 32;
            var x = halfX - subtractX;
            var y = halfY - subtractY;
            
            var dir = point.Direction;

            var position = dir switch
            {
                _ when dir == Vector2I.Up => new Vector2(x, Size.Y - 64),
                _ when dir ==Vector2I.Down => new Vector2(x, -64),
                _ when dir ==Vector2I.Left => new Vector2(0, y),
                _ when dir ==Vector2I.Right => new Vector2(Size.X - 64, y),
                _ => entryPoint.Position // Default case if none match
            };

            entryPoint.Position = position;

            AddChild(entryPoint);
        }
    }

    public static Room Create(Vector2I position, Vector2I size, Bounds bounds)
    {
        var room = GD.Load<PackedScene>("res://Scenes/Levels/Dungeon/Room.tscn").Instantiate<Room>();

        room.Position = position;
        room.Bounds = bounds;
        room.Size = size;
        room.Background.Size = size;
        room.Label.Position = size / 2;

        return room;
    }
}