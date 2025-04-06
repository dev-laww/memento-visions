using Godot;
using GodotUtilities;

namespace Game.World;

[Tool]
[Scene]
public partial class Hallway : Node2D
{
    [Node] private Label Label;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public static Hallway Create(Vector2I position)
    {
        var hallway = GD.Load<PackedScene>("res://Scenes/World/Levels/Dungeon/Hallway.tscn").Instantiate<Hallway>();
        hallway.GlobalPosition = position;

        return hallway;
    }
}