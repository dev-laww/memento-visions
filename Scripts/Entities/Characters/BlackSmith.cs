using Game.Autoload;
using Game.Components;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.Entities;

[Scene]
public partial class BlackSmith : Entity
{
    [Node] private AnimationTree animationTree;
    [Node] private Timer hammerUpTimer;

    private AnimationNodeStateMachinePlayback playback;
    private string currentAnimation = "idle";

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void OnReady()
    {
        hammerUpTimer.Timeout += OnHammerUpTimeout;

        playback = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");
    }

    private void OnHammerUpTimeout()
    {
        playback.Travel(currentAnimation);
        hammerUpTimer.Call("start_random");
        currentAnimation = currentAnimation == "idle" ? "idle_hammer" : "idle";
    }

    public void Work()
    {
        playback.Travel("work");
    }

    public void Idle()
    {
        playback.Travel("idle");
    }
}