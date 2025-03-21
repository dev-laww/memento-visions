using Godot;
using GodotUtilities;

namespace Game.UI.Common;

[Scene]
public partial class DashIndicator : TextureProgressBar
{
    public bool Running => valueTween?.IsRunning() ?? false;
    private Tween valueTween;
    private Tween tween;
    private float duration = 1f;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public void Start(float dashCoolDown)
    {
        duration = dashCoolDown;


        valueTween?.KillIfValid();
        valueTween = CreateTween();

        valueTween.TweenProperty(this, "value", 100, Mathf.Max(0f, duration - 1f)).From(0f)
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Cubic);
        valueTween.TweenCallback(Callable.From(OnTweenFinished));

        tween?.KillIfValid();
        tween = CreateTween();

        tween.TweenProperty(this, "scale", new Vector2(2.5f, 0.8f), 0.05f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine).From(Vector2.Zero);
        tween.TweenProperty(this, "scale", new Vector2(0.8f, 2f), 0.1f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
        tween.TweenProperty(this, "scale", Vector2.One, 0.15f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
        tween.TweenInterval(Mathf.Max(0f, duration - 1f));
    }

    private void OnTweenFinished()
    {
        tween = CreateTween();

        tween.TweenProperty(this, "scale", new Vector2(2.5f, 0.8f), 0.05f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine).From(Vector2.One);
        tween.TweenProperty(this, "scale", new Vector2(0.8f, 2f), 0.1f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
        tween.TweenProperty(this, "scale", Vector2.One, 0.15f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
    }
}

