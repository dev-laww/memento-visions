using Godot;
using GodotUtilities;

namespace Game;

[Tool]
[Scene]
public partial class Circle : Node2D
{
    [Export] private float radius = 100;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Process(double delta)
    {
        QueueRedraw();
    }

    public override void _Draw()
    {
        var color = Colors.White;

        color.A = 0.01f;

        DrawCircle(Vector2.Zero, radius, color);
    }
}

