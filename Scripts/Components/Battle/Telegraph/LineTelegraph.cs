using Godot;
using GodotUtilities;

namespace Game.Components;

[Tool]
[Scene]
public partial class LineTelegraph : Node2D
{
    [Node] private Line2D line2d;

    [Export] public float Duration = 1.75f;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        var color = Colors.White;
        color.A = GetParent() is TelegraphCanvas ? 0.01f : 1;

        line2d.SelfModulate = color;
        line2d.NotifyPropertyListChanged();
    }

    public void Start(Vector2 start, Vector2 end, float width = 16f)
    {
        line2d.Points = [];
        GlobalPosition = start;


        var trajectory = end - start;
        var length = trajectory.Length();
        line2d.Rotation = trajectory.Angle();
        line2d.Width = width;

        var tweenMethodDuration = Duration * (0.5f / 1.75f);
        var tweenIntervalDuration = Duration * (1f / 1.75f);
        var scaleDuration = Duration * (0.25f / 1.75f);

        var tween = CreateTween();
        tween.TweenMethod(Callable.From((float t) => SetupPoints(t, length)), 0f, 1f, tweenMethodDuration).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Sine);
        tween.TweenInterval(tweenIntervalDuration);
        tween.TweenProperty(line2d, "scale", new Vector2(1, 0), scaleDuration).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
        tween.TweenCallback(Callable.From(QueueFree));
    }

    private void SetupPoints(float t, float length)
    {
        line2d.Points = [Vector2.Zero, Vector2.Right * length * t];
    }
}

