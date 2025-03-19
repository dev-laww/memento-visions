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
    private const float PLAYER_DISTANCE = 250;
    private const float MOVE_RANGE = 300;

    [Node] private VelocityManager velocityManager;
    [Node] private PathFindManager pathFindManager;
    [Node] private AnimationTree animationTree;
    [Node] private Timer moveTimer;
    [Node] private Timer specialAttackTimer;
    [Node] private Timer commonAttackTimer;
    [Node] private HitBox hitBox;

    private AnimationNodeStateMachinePlayback playback;
    private Vector2[] directions = { Vector2.Up, Vector2.Down, Vector2.Left, Vector2.Right };
    private Vector2 initialPosition;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void OnReady()
    {
        playback = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");

        StateMachine.AddStates(Normal, EnterNormal, LeaveNormal);
        StateMachine.AddStates(Move, EnterMove, LeaveMove);
        StateMachine.AddStates(CommonAttack, EnterCommonAttack, LeaveCommonAttack);
        StateMachine.AddStates(SpecialAttack, EnterSpecialAttack);

        StateMachine.SetInitialState(Normal);
        animationTree.AnimationFinished += OnAnimationFinished;
        initialPosition = GlobalPosition;
    }

    public override void OnProcess(double delta)
    {
        velocityManager.ApplyMovement();
    }

    private void EnterNormal()
    {
        EnterState(IDLE);

        if (moveTimer.Paused)
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

        // if (specialAttackTimer.IsStopped())
        // {
        //     StateMachine.ChangeState(SpecialAttack);
        // }

        // if (commonAttackTimer.IsStopped())
        // {
        //     StateMachine.ChangeState(CommonAttack);
        // }
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

        var randomNumber = MathUtil.RNG.RandiRange(0, directions.Length - 1);
        var moveDirection = directions[randomNumber];
        var moveLength = MathUtil.RNG.RandfRange(50, 150);
        var targetPosition = GlobalPosition + moveDirection * moveLength;

        if (targetPosition.DistanceSquaredTo(initialPosition) > MOVE_RANGE * MOVE_RANGE)
        {
            var direction = (initialPosition - GlobalPosition).Normalized();

            if (Mathf.Abs(direction.X) > Mathf.Abs(direction.Y))
            {
                targetPosition = GlobalPosition + new Vector2(Mathf.Sign(direction.X), 0) * moveLength;
            }
            else
            {
                targetPosition = GlobalPosition + new Vector2(0, Mathf.Sign(direction.Y)) * moveLength;
            }
        }

        pathFindManager.SetTargetPosition(targetPosition);
    }

    private void Move()
    {
        UpdateBlendPosition();

        if (pathFindManager.NavigationAgent2D.IsNavigationFinished())
        {
            StateMachine.ChangeState(Normal);
        }

        pathFindManager.Follow();
    }

    private void LeaveMove()
    {
        moveTimer.Call(START_RANDOM);
    }

    private void EnterCommonAttack()
    {
        EnterState(COMMON_ATTACK);
        var playerPosition = this.GetPlayer()?.GlobalPosition ?? GlobalPosition;
        pathFindManager.SetTargetPosition(playerPosition);
    }

    private void CommonAttack()
    {
        pathFindManager.Follow();

        if (!pathFindManager.NavigationAgent2D.IsNavigationFinished()) return;

        StateMachine.ChangeState(Normal);
    }

    private void LeaveCommonAttack()
    {
        commonAttackTimer.Call(START_RANDOM);
    }

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

    private void OnAnimationFinished(StringName anim)
    {
        if (!anim.ToString().Contains("attack")) return;

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