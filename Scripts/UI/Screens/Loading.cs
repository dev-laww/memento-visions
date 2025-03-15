using System.Threading.Tasks;
using Game.Common;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.UI.Screens;

[Scene]
public partial class Loading : CanvasLayer
{
    public enum Transition
    {
        Fade
    }

    [Node] private Panel panel;
    [Node] private ProgressBar progressBar;
    [Node] private Timer timer;
    [Node] public AnimationPlayer animationPlayer;

    private string currentTransition;

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

    public void SetProgress(float value)
    {
        var tween = CreateTween();
        tween.TweenProperty(progressBar, "value", value, 0.2f);
    }

    public async Task Start(Transition transition)
    {
        var transitionName = transition.ToAnimation();

        if (!animationPlayer.HasAnimation(transitionName))
        {
            Log.Warn($"Animation '{transitionName}' not found.");
            return;
        }

        animationPlayer.Play(transitionName);
        timer.Start();
        currentTransition = transitionName;

        await ToSignal(animationPlayer, "animation_finished");
    }

    public async Task End()
    {
        animationPlayer.PlayBackwards(currentTransition);
        timer.Stop();
        currentTransition = null;

        await ToSignal(animationPlayer, "animation_finished");

        QueueFree();
    }
}