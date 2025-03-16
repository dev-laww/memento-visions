using Game.Components;
using Game.Utils.Extensions;
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

    public override void OnPhysicsProcess(double delta)
    {
        velocityManager.ApplyMovement();
    }

    public void EnterMove()
    {
        EnterState(MOVE);
    }

    private void Move()
    {
        // var velocity 

        pathFindManager.SetTargetPosition(this.GetPlayer()?.GlobalPosition ?? Vector2.Zero);
        pathFindManager.Follow();
    }


    public void EnterCommonAttack()
    {
        EnterState(COMMON_ATTACK);
    }

    private void CommonAttack()
    {
        // GD.Print(COMMON_ATTACK);
    }


    public void EnterSpecialAttack()
    {
        EnterState(SPECIAL_ATTACK);
    }

    private void SpecialAttack()
    {
        // GD.Print(SPECIAL_ATTACK);
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

    private void EnterState(string state)
    {
        playback.Travel(state);
        UpdateBlendPosition();
    }

    private void UpdateBlendPosition()
    {
        animationTree.Set("parameters/Move/blend_position", velocityManager.LastFacedDirection);
        animationTree.Set("parameters/Common Attack/blend_position", velocityManager.LastFacedDirection);
        animationTree.Set("parameters/Special Attack/blend_position", velocityManager.LastFacedDirection);
    }
}

