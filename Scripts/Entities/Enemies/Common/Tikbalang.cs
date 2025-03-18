using Game.Components;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.Entities;

[Scene]
public partial class Tikbalang : Enemy
{
    private const string START_RANDOM = "start_random";
    private const string SPECIAL_ATTACK = "Special Attack";
    private const string COMMON_ATTACK = "Common Attack";
    private const string IDLE = "Idle";
    private const string MOVE = "Move";
    private const float DISTANCE_TO_PLAYER = 16;

    [Node] private VelocityManager velocityManager;
    [Node] private PathFindManager pathFindManager;
    [Node] private AnimationTree animationTree;
    // [Node] private HitBox hitBox;

    [Node] private Timer moveTimer;

    private AnimationNodeStateMachinePlayback playback;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void OnReady()
    {
        playback = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");

        StateMachine.AddStates(Normal, EnterNormal, LeaveNormal);
        StateMachine.AddStates(Move, EnterMove);
        StateMachine.AddStates(CommonAttack, EnterCommonAttack);
        StateMachine.AddStates(SpecialAttack, EnterSpecialAttack);

        StateMachine.SetInitialState(Move);
    }

    public override void OnProcess(double delta)
    {
        velocityManager.ApplyMovement();
    }

    private void EnterNormal()
    {
        EnterState(IDLE);

        if (moveTimer.IsStopped())
        {
            moveTimer.Paused = false;
        }
    }

    private void Normal()
    {
        UpdateBlendPosition();
        velocityManager.Decelerate();

        if (moveTimer.IsStopped())
        {
            StateMachine.ChangeState(Move);
        }
    }

    private void LeaveNormal()
    {
        if (!moveTimer.IsStopped())
        {
            moveTimer.Paused = true;
        }
    }

    private void EnterMove()
    {
        EnterState(MOVE);

        var player = this.GetPlayer();
        var playerPosition = player?.GlobalPosition ?? GlobalPosition;

        pathFindManager.SetTargetPosition(playerPosition);
    }

    private void Move()
    {
        UpdateBlendPosition();

        var player = this.GetPlayer();
        var isPlayerInRange = player != null && player.GlobalPosition.DistanceSquaredTo(GlobalPosition) <
            DISTANCE_TO_PLAYER * DISTANCE_TO_PLAYER;

        if (isPlayerInRange)
        {
            var randomNumber = MathUtil.RNG.RandfRange(0, 1);
            moveTimer.Call(START_RANDOM);

            if (randomNumber < 0.4) // 40% chance
            {
                StateMachine.ChangeState(SpecialAttack);
            }
            else
            {
                StateMachine.ChangeState(CommonAttack);
            }
        }

        if (pathFindManager.NavigationAgent2D.IsNavigationFinished())
        {
            moveTimer.Call(START_RANDOM);

            if (isPlayerInRange)
            {
                var randomNumber = MathUtil.RNG.RandfRange(0, 1);

                if (randomNumber < 0.4) // 40% chance
                {
                    StateMachine.ChangeState(SpecialAttack);
                }
                else
                {
                    StateMachine.ChangeState(CommonAttack);
                }
            }
            else
            {
                StateMachine.ChangeState(Normal);
            }
        }

        pathFindManager.Follow();
    }

    private void EnterCommonAttack()
    {
        EnterState(COMMON_ATTACK);
        velocityManager.Decelerate(force: true);

        // knockback player
    }

    private void CommonAttack() { }

    private void EnterSpecialAttack()
    {
        EnterState(SPECIAL_ATTACK);
        velocityManager.Decelerate(force: true);

        // apply stun to player
    }

    private void SpecialAttack() { }


    private void EnterState(string state)
    {
        playback.Travel(state);
        UpdateBlendPosition();
    }

    private void ChangeStateToNormal()
    {
        StateMachine.ChangeState(Normal);
    }

    private void UpdateBlendPosition()
    {
        animationTree.Set("parameters/Move/blend_position", velocityManager.LastFacedDirection);
        animationTree.Set("parameters/Common Attack/blend_position", velocityManager.LastFacedDirection);
        animationTree.Set("parameters/Special Attack/blend_position", velocityManager.LastFacedDirection);
        animationTree.Set("parameters/Idle/blend_position", velocityManager.LastFacedDirection);
    }
}