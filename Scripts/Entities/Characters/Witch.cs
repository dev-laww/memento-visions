using Godot;
using GodotUtilities;

namespace Game.Entities;

[Scene]
public partial class Witch : Entity
{
    [Node] private Timer tapTimer;
    [Node] private AnimationTree animationTree;

    private AnimationNodeStateMachinePlayback playback;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void OnReady()
    {
        playback = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");

        tapTimer.Timeout += OnTapTimeout;
    }

    private void OnTapTimeout()
    {
        playback.Travel("tap");
        tapTimer.Call("start_random");
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
