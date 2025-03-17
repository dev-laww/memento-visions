using Game.Common.Extensions;
using Game.Components;
using Game.Utils;
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
    [Node] private VelocityManager velocityManager;
    [Node] private PathFindManager pathFindManager;
    [Node] private Timer specialAttackTimer;
    [Node] private Timer specialAttackWindUpTimer;
    [Node] private Timer patrolTimer;
    [Node] private StatsManager statsManager;

    private AnimationNodeStateMachinePlayback playback;
    private Vector2 initialPosition;
    private Vector2 chargeOrigin;
    private Vector2 chargeDestination;
    private Vector2 chargeDirection;
    private Damage damageComponent;
    private bool isShowingAttackIndicator;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void OnReady()
    {
        playback = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");

        StateMachine.AddStates(Normal, EnterNormal, LeaveNormal);
        StateMachine.AddStates(Patrol, EnterPatrol);
        StateMachine.AddStates(AttackWindUp, EnterAttackWindUp);
        StateMachine.AddStates(Attack, EnterAttack, LeaveAttack);

        StateMachine.SetInitialState(Patrol);
    }

    public override void OnProcess(double delta)
    {
        velocityManager.ApplyMovement();
    }

    private void EnterNormal()
    {
        if (specialAttackTimer.Paused)
        {
            specialAttackTimer.Paused = false;
        }

        patrolTimer.Start();
    }

    private void Normal()
    {
        if (specialAttackTimer.IsStopped())
        {
            StateMachine.ChangeState(AttackWindUp);
        }

        if (patrolTimer.IsStopped())
        {
            StateMachine.ChangeState(Patrol);
        }
    }

    private void LeaveNormal()
    {
        if (!specialAttackTimer.IsStopped())
        {
            specialAttackTimer.Paused = true;
        }
    }

    private void EnterPatrol()
    {
        var randomDirection = MathUtil.RNG.RandDirection();
        var randomLength = MathUtil.RNG.RandfRange(50, 100);
        var randomAngle = MathUtil.RNG.RandfRange(0f, 360f);

        var targetPosition = initialPosition + (randomDirection * randomLength);
        targetPosition.RotatedDegrees(randomAngle);

        pathFindManager.SetTargetPosition(targetPosition);
    }

    private void Patrol()
    {
        pathFindManager.Follow();

        if (pathFindManager.NavigationAgent2D.IsNavigationFinished())
        {
            StateMachine.ChangeState(Normal);
        }
    }

    private void EnterAttackWindUp()
    {
        isShowingAttackIndicator = false;
        chargeOrigin = GlobalPosition;
    }

    private void AttackWindUp()
    {
        velocityManager.Decelerate();

        if (specialAttackWindUpTimer.IsStopped() && !isShowingAttackIndicator && velocityManager.Velocity.LengthSquared() < 20 * 20)
        {
            isShowingAttackIndicator = true;
            specialAttackWindUpTimer.Start();

            var playerPostion = this.GetPlayer()?.GlobalPosition ?? GlobalPosition;
            var direction = (playerPostion - chargeOrigin).TryNormalize();

            chargeDestination = chargeOrigin + direction * (playerPostion.DistanceTo(chargeOrigin) - 32);
            chargeDirection = (chargeDestination - chargeOrigin).TryNormalize();

            var canvas = GetTree().Root.GetFirstChildOrNull<TelegraphCanvas>();

            new TelegraphFactory.LineTelegraphBuilder(canvas, chargeOrigin)
                .SetDestitnation(chargeDestination)
                .Build();
        }
        else if (specialAttackWindUpTimer.IsStopped() && isShowingAttackIndicator)
        {
            StateMachine.ChangeState(Attack);
        }
    }

    private void EnterAttack()
    {
        StatsManager.ApplySpeedModifier("special_attack", 2f);
        pathFindManager.NavigationAgent2D.AvoidanceEnabled = false;

        new DamageFactory.LineDamageBuilder(chargeOrigin, chargeDestination)
            .SetOwner(this)
            .SetDamage(statsManager.Damage * .4f)
            .SetDuration(0.1f)
            .Build();
    }

    private void Attack()
    {
        velocityManager.Accelerate(chargeDirection);

        if (GlobalPosition.DistanceSquaredTo(chargeDestination) > 32 * 32) return;

        StateMachine.ChangeState(Normal);
    }

    private void LeaveAttack()
    {
        EnterState(SPECIAL_ATTACK); // TODO: Make this a random attack e.g. Common Attack, Special Attack

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
}

