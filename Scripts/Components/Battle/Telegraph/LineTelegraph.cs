using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Game;

[Tool]
public partial class LineTelegraph : Line2D
{
    [Signal] public delegate void FinishedEventHandler();

    [Export] public Vector2 TargetPosition;

    public override void _Ready()
    {
        var tween = CreateTween();

        AddPoints();

        tween.TweenProperty(this, "width", Width, 0.3f).From(5)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);

    }

    public void SetTargetPosition(Vector2 position)
    {
        TargetPosition = position;
    }

    public void End()
    {
        var tween = CreateTween();

        tween.TweenProperty(this, "width", 0, 0.3f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);

        tween.TweenCallback(Callable.From(OnTweenFinish));
    }

    private void OnTweenFinish()
    {
        EmitSignalFinished();
    }

    private async void AddPoints()
    {
        var length = Vector2.Zero.DistanceTo(TargetPosition);
        var angle = Vector2.Zero.AngleToPoint(TargetPosition);
        var pointsToDraw = new List<Vector2>();

        for (var i = 0; i < length; i += 5)
        {
            var x = Mathf.Cos(angle) * i;
            var y = Mathf.Sin(angle) * i;

            pointsToDraw.Add(new Vector2(x, y));
        }

        AddPoint(Vector2.Zero);

        foreach (var point in pointsToDraw)
        {
            AddPoint(point);

            QueueRedraw();

            await ToSignal(GetTree().CreateTimer(0.01f), "timeout");
        }
    }
}

