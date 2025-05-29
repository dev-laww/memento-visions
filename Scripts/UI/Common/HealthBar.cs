using Game.Components;
using Godot;
using GodotUtilities;

namespace Game.UI.Common;

[Tool]
[Scene]
public partial class HealthBar : ProgressBar
{
    [Node] private ProgressBar damageBar;
    [Node] private Timer timer;

    private float health;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        timer.Timeout += OnTimerTimeout;
    }

    public void Initialize(StatsManager statsManager)
    {
        MaxValue = statsManager.MaxHealth;
        Value = statsManager.Health;

        damageBar.MaxValue = statsManager.MaxHealth;
        damageBar.Value = statsManager.Health;

        statsManager.StatDecreased += OnStatDecreased;
        statsManager.StatIncreased += OnStatIncreased;
        ColorChange();
    }

    private void OnStatIncreased(float increase, StatsType type)
    {
        if (type != StatsType.Health) return;

        var tween = CreateTween();

        tween.TweenProperty(this, "value", Value + increase, timer.WaitTime)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);
        tween.TweenCallback(Callable.From(() =>
        {
            damageBar.Value = Value;
            ColorChange();
        }));
    }

    private void OnStatDecreased(float decrease, StatsType type)
    {
        if (type != StatsType.Health) return;

        var tween = CreateTween();

        tween.TweenProperty(this, "value", Value - decrease, timer.WaitTime)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);

        tween.TweenCallback(Callable.From(ColorChange));

        timer.Start();
    }

    private void OnTimerTimeout()
    {
        var tween = CreateTween();

        tween.TweenProperty(damageBar, "value", Value, timer.WaitTime)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);

        ColorChange();
    }

    private void ColorChange()
    {
        if (MaxValue <= 0) return;

        if (Value >= MaxValue * 0.7f)
        {
            SelfModulate = Color.FromHtml("#26E54D");
        }
        else if (Value >= MaxValue * 0.3f && Value < MaxValue * 0.7f)
        {
            SelfModulate = Color.FromHtml("#FFFFB3");
        }
        else
        {
            SelfModulate = Color.FromHtml("#E64536");
        }
    }
}