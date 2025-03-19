using Game.Common.Extensions;
using Game.Components;
using Game.Utils;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.Entities;

[Scene]
public partial class Aswang : Enemy
{
    private const string START_RANDOM = "start_random";
    private const string SPECIAL_ATTACK = "Special Attack";
    private const string COMMON_ATTACK = "Common Attack";
    private const float DISTANCE_TO_PLAYER = 200;
    private const float ATTACK_RANGE = 32;

    [Node] private AnimationTree animationTree;
    [Node] private VelocityManager velocityManager;
    [Node] private PathFindManager pathFindManager;
    [Node] private Timer specialAttackTimer;
    [Node] private Timer specialAttackWindUpTimer;
    [Node] private Timer patrolTimer;
    [Node] private StatsManager statsManager;
    [Node] private HitBox hitBox;

    private AnimationNodeStateMachinePlayback playback;
    private Vector2 initialPosition;
    private Vector2 chargeOrigin;
    private Vector2 chargeDestination;
    private Vector2 chargeDirection;
    private Damage damageComponent;
    private bool isShowingAttackIndicator;
    private string attackState;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void OnReady()
    {
        playback = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");
        initialPosition = GlobalPosition;

        StateMachine.AddStates(Normal, EnterNormal, LeaveNormal);
        StateMachine.AddStates(Patrol, EnterPatrol);
        StateMachine.AddStates(AttackWindUp, EnterAttackWindUp);
        StateMachine.AddStates(Attack, EnterAttack, LeaveAttack);

        StateMachine.SetInitialState(Normal);

        hitBox.AddStatusEffect("bleed", 3);
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

        if (patrolTimer.Paused)
        {
            patrolTimer.Paused = false;
        }
    }

    private void Normal()
    {
        velocityManager.Decelerate();
        var player = this.GetPlayer();
        var isPlayerInRange = player != null && player.GlobalPosition.DistanceSquaredTo(GlobalPosition) < DISTANCE_TO_PLAYER * DISTANCE_TO_PLAYER;

        if (specialAttackTimer.IsStopped() && isPlayerInRange)
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

        if (!patrolTimer.IsStopped())
        {
            patrolTimer.Paused = true;
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
            patrolTimer.Call(START_RANDOM);
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

        if (
            specialAttackWindUpTimer.IsStopped() &&
            !isShowingAttackIndicator &&
            velocityManager.Velocity.LengthSquared() < 20 * 20
        )
        {
            isShowingAttackIndicator = true;
            specialAttackWindUpTimer.Start();

            var playerPosition = this.GetPlayer()?.GlobalPosition ?? GlobalPosition;
            var direction = (playerPosition - chargeOrigin).TryNormalize();
            var isGoingUp = direction.Y < 0 && Mathf.Abs(direction.Y) > Mathf.Abs(direction.X);
            var isGoingSideWay = Mathf.Abs(direction.X) > 0.5f;
            var range = ATTACK_RANGE * (isGoingUp ? 2f : 1f);

            chargeDestination = isGoingUp || isGoingSideWay ? chargeOrigin + direction * (playerPosition.DistanceTo(chargeOrigin) - range) : playerPosition;
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
        StatsManager.ApplySpeedModifier("attack", 2f);
        pathFindManager.NavigationAgent2D.AvoidanceEnabled = false;

        var randomNumber = MathUtil.RNG.RandfRange(0, 1);
        attackState = randomNumber < 0.3f ? SPECIAL_ATTACK : COMMON_ATTACK;
        var damage = statsManager.Damage * (attackState == SPECIAL_ATTACK ? 1.2f : 1f);

        hitBox.Damage = damage;

        new DamageFactory.LineDamageBuilder(chargeOrigin, chargeDestination)
            .SetOwner(this)
            .SetDamage(statsManager.Damage * .4f)
            .SetDuration(0.1f)
            .Build();
    }

    private void Attack()
    {
        velocityManager.Accelerate(chargeDirection);

        if (GlobalPosition.DistanceSquaredTo(chargeDestination) > 4 * 4) return;

        StateMachine.ChangeState(Normal);
    }

    private void LeaveAttack()
    {
        EnterState(attackState);

        pathFindManager.NavigationAgent2D.AvoidanceEnabled = true;
        StatsManager.RemoveSpeedModifier("attack");
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