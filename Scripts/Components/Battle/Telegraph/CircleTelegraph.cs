using Godot;
using GodotUtilities;

namespace Game;

[Tool]
[Scene]
public partial class CircleTelegraph : Node2D
{
    [Export] private float radius = 50;

    private float currentRadius;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        var tween = CreateTween();

        tween.TweenProperty(this, "currentRadius", radius, 0.3f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);
    }

    public override void _Process(double delta)
    {
        QueueRedraw();
    }

    public override void _Draw()
    {
        var color = Colors.White;

        color.A = 0.01f;

        DrawCircle(Vector2.Zero, currentRadius, color);
    }
}

