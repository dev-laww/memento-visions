using Godot;

namespace Game.Components;

[Tool]
public partial class CircleTelegraph : Node2D
{
    [Export] private float radius;

    private float currentRadius;
    private Color color;

    public override void _Ready()
    {
        color = Colors.White;
        color.A = GetParent() is TelegraphCanvas ? 0.01f : 1;

        if (Engine.IsEditorHint())
            currentRadius = radius;
    }

    public void Start(Vector2 position)
    {
        GlobalPosition = position;
        currentRadius = 0;

        var tween = CreateTween();
        tween.TweenProperty(this, "currentRadius", radius, .5f).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Sine);
        tween.TweenInterval(1f);
        tween.TweenProperty(this, "currentRadius", 0, .25f).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
        tween.TweenCallback(Callable.From(QueueFree));
    }

    public override void _Draw()
    {
        DrawCircle(Vector2.Zero, currentRadius, color);
    }

    public override void _Process(double delta)
    {
        if (currentRadius == radius) return;

        QueueRedraw();
    }
}

