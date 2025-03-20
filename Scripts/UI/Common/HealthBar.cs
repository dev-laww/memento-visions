using Game.Components;
using Godot;
using GodotUtilities;

namespace Game;

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

    private void Initialize(StatsManager statsManager)
    {
        Value = statsManager.Health;
        MaxValue = statsManager.MaxHealth;

        damageBar.MaxValue = statsManager.MaxHealth;
        damageBar.Value = statsManager.Health;

        statsManager.StatDecreased += OnStatDecreased;
        statsManager.StatIncreased += OnStatIncreased;
    }

    private void OnStatIncreased(float value, StatsType type)
    {
        if (type != StatsType.Health) return;

        var tween = CreateTween();

        damageBar.Value = Value;
        tween.TweenProperty(this, "value", value, 0.3f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);
    }

    private void OnStatDecreased(float value, StatsType type)
    {
        if (type != StatsType.Health) return;

        var tween = CreateTween();

        tween.TweenProperty(this, "value", value, 0.3f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);

        timer.Start();
    }

    private void OnTimerTimeout()
    {
        var tween = CreateTween();

        tween.TweenProperty(damageBar, "value", Value, 0.3f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);
    }
}

