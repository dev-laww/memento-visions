using Game.Globals;
using Godot;
using GodotUtilities;

namespace Game;

[Scene]
public partial class LoadingScreen : Node2D
{
    [Signal]
    public delegate void TransitionInCompleteEventHandler();

    [Node]
    private ProgressBar progressBar;

    [Node]
    public AnimationPlayer animationPlayer;

    [Node]
    private Timer timer;

    private Transition transition;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        progressBar.Visible = false;
        timer.Timeout += () => progressBar.Visible = true;
    }

    public void StartTransition(Transition _transition)
    {
        if (!animationPlayer.HasAnimation(_transition.ToValue()))
        {
            GD.PushWarning($"{_transition} does not exists");
            transition = _transition;
        }

        transition = _transition;
        animationPlayer.Play(transition.ToValue());
        timer.Start();
    }

    public async void FinishTransition()
    {
        timer?.Stop();

        var endingTransitionName = transition.ToValue().Replace("to", "from");
        animationPlayer.Play(endingTransitionName);

        await ToSignal(animationPlayer, "animation_finished");
        QueueFree();
    }

    public void UpdateBar(float value)
    {
        var tween = CreateTween();
        tween.TweenProperty(timer, "value", value, 0.2f);
    }

    public void ReportMidpoint() => EmitSignal(SignalName.TransitionInComplete);
}