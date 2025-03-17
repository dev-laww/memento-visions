using System.Runtime.CompilerServices;
using Game.Autoload;
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
    const float PATROL_WAIT_TIME = 3f;


    [Node] private AnimationTree animationTree;
    [Node] private Timer specialAttackTimer;
    [Node] private VelocityManager velocityManager;
    [Node] private PathFindManager pathFindManager;

    private AnimationNodeStateMachinePlayback playback;
    private Vector2 initialPosition;
    private Vector2 chargeDestination;
    private Vector2 chargeDirection;
    private Damage damageComponent;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void OnReady()
    {
        playback = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");

        StateMachine.AddStates(Patrol, EnterPatrol, LeavePatrol);
        StateMachine.AddStates(SpecialAttack, EnterSpecialAttack, LeaveSpecialAttack);

        StateMachine.SetInitialState(Patrol);

        pathFindManager.NavigationAgent2D.NavigationFinished += OnPathFindNavigationFinished;
    }

    public override void OnProcess(double delta)
    {
        velocityManager.ApplyMovement();
    }

    private async void EnterPatrol()
    {
        EnterState(MOVE);

        var randomDirection = MathUtil.RNG.RandDirection();
        var randomLength = MathUtil.RNG.RandfRange(50, 100);
        var randomAngle = MathUtil.RNG.RandfRange(0f, 360f);

        var targetPosition = initialPosition + (randomDirection * randomLength);
        targetPosition.RotatedDegrees(randomAngle);

        pathFindManager.SetTargetPosition(targetPosition);

        if (specialAttackTimer.Paused)
        {
            specialAttackTimer.Paused = false;
        }

        await ToSignal(GetTree().CreateTimer(PATROL_WAIT_TIME), "timeout");
    }

    private void Patrol()
    {
        pathFindManager.Follow();

        if (specialAttackTimer.IsStopped())
        {
            StateMachine.ChangeState(SpecialAttack);
        }
    }

    private void LeavePatrol()
    {
        if (!specialAttackTimer.IsStopped())
        {
            specialAttackTimer.Paused = true;
        }
    }

    private void EnterSpecialAttack()
    {
        chargeDestination = this.GetPlayer()?.GlobalPosition ?? GlobalPosition;

        pathFindManager.NavigationAgent2D.AvoidanceEnabled = false;
        chargeDirection = (chargeDestination - GlobalPosition).TryNormalize();
        StatsManager.ApplySpeedModifier("special_attack", 2f);
    }

    private void SpecialAttack()
    {
        velocityManager.Accelerate(chargeDirection);

        if (GlobalPosition.DistanceSquaredTo(chargeDestination) > 32 * 32) return;

        StateMachine.ChangeState(Patrol);
    }

    private void LeaveSpecialAttack()
    {
        pathFindManager.NavigationAgent2D.AvoidanceEnabled = true;
        StatsManager.RemoveSpeedModifier("special_attack");
        specialAttackTimer.Call(START_RANDOM);
        initialPosition = GlobalPosition;
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

    private async void OnPathFindNavigationFinished()
    {
        var state = StateMachine.GetCurrentState();

        if (state != Patrol) return;

        await ToSignal(GetTree().CreateTimer(PATROL_WAIT_TIME), "timeout");

        StateMachine.ChangeState(Patrol);
    }
}

