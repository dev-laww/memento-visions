using Godot;
using GodotUtilities;

namespace Game.Entities;

[Scene]
public partial class StoryTeller : Entity
{
    private const string IDLE = "idle";
    private const string LOOK = "look";
    private const string FIX_HAT = "fix_hat";
    private const string ENTER_WORK = "work_entry";
    private const string WORK = "work";
    private const string EXIT_WORK = "work_exit";
    private const string START_RANDOM = "start_random";

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

        actionTimer.Timeout += OnTimeout;
    }

    private void OnTimeout()
    {
        var randomNumber = MathUtil.RNG.RandfRange(0, 1);

        switch (randomNumber)
        {
            case < 0.1f:
                Work();
                GetTree().CreateTimer(MathUtil.RNG.RandfRange(1, 2)).Timeout += ExitWork;
                break;
            case < 0.5f:
                playback.Travel(LOOK);
                break;
            default:
                playback.Travel(FIX_HAT);
                break;
        }
    }

    private void Work()
    {
        playback.Travel(ENTER_WORK);
    }

    private void ExitWork()
    {
        playback.Travel(EXIT_WORK);
    }
}

