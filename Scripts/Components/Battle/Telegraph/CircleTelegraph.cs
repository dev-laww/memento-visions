using Godot;
using GodotUtilities;

namespace Game;

[Tool]
public partial class CircleTelegraph : Node2D
{
    [Export] public float Radius = 50;

    [Signal] public delegate void FinishedEventHandler();

    private float currentRadius;

    public override void _Ready()
    {
        var tween = CreateTween();

        tween.TweenProperty(this, "currentRadius", Radius, 0.3f)
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

    public void End()
    {
        var tween = CreateTween();

        tween.TweenProperty(this, "currentRadius", 0, 0.3f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);

        tween.TweenCallback(Callable.From(OnTweenFinish));
    }

    private void OnTweenFinish()
    {
        EmitSignalFinished();
    }
}

