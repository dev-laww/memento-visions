using Game.Components;
using Godot;
using GodotUtilities;

namespace Game.Entities;

[Scene]
public partial class Aswang : Entity
{
    const string START_RANDOM = "start_random";
    const string SPECIAL_ATTACK = "Special Attack";
    const string COMMON_ATTACK = "Common Attack";
    const string MOVE = "Move";


    [Node] private AnimationTree animationTree;
    [Node] private Timer attackTimer;
    [Node] private VelocityManager velocityManager;
    [Node] private PathFindManager pathFindManager;

    private AnimationNodeStateMachinePlayback playback;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void OnReady()
    {
        playback = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");

        StateMachine.AddStates(Move, EnterMove);
        StateMachine.AddStates(CommonAttack, EnterCommonAttack);
        StateMachine.AddStates(SpecialAttack, EnterSpecialAttack);
        StateMachine.SetInitialState(Move);

        attackTimer.Timeout += OnAttackTimerTimeout;
    }

    public void EnterMove()
    {
        EnterState(MOVE);
        playback.Travel(MOVE);
    }

    private void Move()
    {
        GD.Print(MOVE);
    }


    public void EnterCommonAttack()
    {
        EnterState(COMMON_ATTACK);
    }

    private void CommonAttack()
    {
        GD.Print(COMMON_ATTACK);
    }


    public void EnterSpecialAttack()
    {
        EnterState(SPECIAL_ATTACK);
    }

    private void SpecialAttack()
    {
        GD.Print(SPECIAL_ATTACK);
    }

    private void OnAttackTimerTimeout()
    {
        var randomNumber = MathUtil.RNG.RandfRange(0, 1);

        if (randomNumber < 0.7f)
        {
            StateMachine.ChangeState(CommonAttack);
        }
        else
        {
            StateMachine.ChangeState(SpecialAttack);
        }
    }

    private void ChangeToMove()
    {
        StateMachine.ChangeState(Move);
    }

    private void EnterState(string animation)
    {
        playback.Travel(animation);
        animationTree.Set($"parameters/{animation}/blend_position", velocityManager.LastFacedDirection);
    }
}

