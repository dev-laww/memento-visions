using Godot;
using GodotUtilities;

namespace Game.Entities;

[Scene]
public partial class Chief : Entity
{
    private const string IDLE = "idle";
    private const string SCRATCH = "scratch";
    private const string EAT = "eat";

    [Node] private Timer actionTimer;
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

        actionTimer.Timeout += OnTimerTimeout;
        actionTimer.Call("start_random");
    }

    private void OnTimerTimeout()
    {
        var randomNumber = MathUtil.RNG.RandfRange(0, 1);

        switch (randomNumber)
        {
            case < 0.2f:
                playback.Travel(EAT);
                break;
            case < 0.5f:
                playback.Travel(SCRATCH);
                break;
            default:
                playback.Travel(IDLE);
                break;
        }
        actionTimer.Call("start_random");
    }
}

