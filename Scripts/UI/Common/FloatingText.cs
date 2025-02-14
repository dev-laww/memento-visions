using System;
using Godot;
using GodotUtilities;

namespace Game;

[Scene]
public partial class FloatingText : Node2D
{
    [Node] private CenterContainer centerContainer;
    [Node] private Label label;

    [Signal] public delegate void FinishedEventHandler();

    private float accumulatedDamage;

    private Tween tween;
    private Tween positionTween;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public void Start(float duration = 1f)
    {
        tween?.KillIfValid();
        positionTween?.KillIfValid();

        tween = CreateTween();
        tween.TweenProperty(this, "scale", new Vector2(2.5f, 0.8f), 0.05f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine).From(Vector2.Zero);
        tween.TweenProperty(this, "scale", new Vector2(0.8f, 2f), 0.1f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
        tween.TweenProperty(this, "scale", Vector2.One, 0.15f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
        tween.TweenInterval(Mathf.Max(0f, duration - 1f));

        positionTween = CreateTween();
        positionTween.TweenProperty(this, "global_position", GlobalPosition + (Vector2.Up * 16), 0.3f).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
        positionTween.TweenProperty(this, "global_position", GlobalPosition + (Vector2.Up * 48), duration - 0.3f).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Circ);

        positionTween.TweenCallback(Callable.From(OnTweenCompleted));
    }

    public void AddDamage(float damage)
    {
        accumulatedDamage += damage;
        var displayValue = accumulatedDamage < 1 ? string.Format("{0:0.00}", accumulatedDamage) : $"{Math.Floor(accumulatedDamage)}";
        SetText(displayValue);
    }

    public void SetText(string text)
    {
        label.Text = text;
    }

    private void OnTweenCompleted()
    {
        tween?.KillIfValid();
        positionTween?.KillIfValid();
        accumulatedDamage = 0;
        EmitSignalFinished();
    }

    public void SetColor(Color color)
    {
        label.SelfModulate = color;
    }
}

