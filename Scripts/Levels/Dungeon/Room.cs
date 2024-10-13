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
        var entryPointsAsStrings = Bounds.EntryPoints.Where(e => e.Open).Select(point => point.Position.ToString()).ToArray();
        Label.Text = $"Position: {Bounds.Rect.Position}\nEntry Points: {string.Join("\n ", entryPointsAsStrings)}";
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