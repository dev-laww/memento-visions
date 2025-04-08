using Game.Common.Extensions;
using Game.Components;
using Game.Data;
using Game.Utils;
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
    private const float PLAYER_DISTANCE = 200;
    private const float MOVE_RANGE = 300;
    private const float HITBOX_SIZE = 40;

    [Node] private VelocityManager velocityManager;
    [Node] private PathFindManager pathFindManager;
    [Node] private AnimationTree animationTree;
    [Node] private Timer moveTimer;
    [Node] private Timer attackTimer;
    [Node] private Timer attackCooldownTimer;
    [Node] private AudioStreamPlayer2D sfxAttack;
    [Node] private AudioStreamPlayer2D sfxSpecial;

    private AnimationNodeStateMachinePlayback playback;
    private Vector2[] directions = [Vector2.Up, Vector2.Down, Vector2.Left, Vector2.Right];
    private Vector2 initialPosition;
    private bool IsPlayerInRange => GlobalPosition.DistanceTo(this.GetPlayer()?.GlobalPosition ?? GlobalPosition) < PLAYER_DISTANCE;
    private bool damageCreated;

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
        StateMachine.AddStates(TravelToPlayer, EnterTravelToPlayer, ExitTravelToPlayer);
        StateMachine.AddStates(SpecialAttack, EnterSpecialAttack, LeaveSpecialAttack);
        StateMachine.AddStates(CommonAttack, EnterCommonAttack, LeaveCommonAttack);

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

        moveTimer.Resume();
        attackTimer.Resume();
    }

    private void Normal()
    {
        UpdateBlendPosition();
        velocityManager.Decelerate();

        if (moveTimer.IsStopped())
        {
            StateMachine.ChangeState(Move);
        }

        if (attackTimer.IsStopped() && attackCooldownTimer.IsStopped() && IsPlayerInRange)
        {
            StateMachine.ChangeState(TravelToPlayer);
        }
    }

    private void LeaveNormal()
    {
        moveTimer.Pause();
        attackTimer.Pause();
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

    private void EnterTravelToPlayer()
    {
        EnterState(MOVE);

        var player = this.GetPlayer();

        if (player is null)
        {
            StateMachine.ChangeState(Normal);
            return;
        }

        var playerPosition = player.GlobalPosition;

        pathFindManager.ForceSetTargetPosition(playerPosition);
        StatsManager.ApplySpeedModifier("travel_to_player", 0.5f);
    }

    private void ExitTravelToPlayer()
    {
        StatsManager.RemoveSpeedModifier("travel_to_player");
    }

    private void TravelToPlayer()
    {
        UpdateBlendPosition();

        var targetPosition = pathFindManager.GetTargetPosition();
        var distance = targetPosition.DistanceSquaredTo(GlobalPosition);
        var navigationFinished = pathFindManager.NavigationAgent2D.IsNavigationFinished();

        if ((distance < 32 * 32 || navigationFinished) && attackCooldownTimer.IsStopped())
        {
            var randomNumber = MathUtil.RNG.RandfRange(0, 1);
            StateMachine.ChangeState(randomNumber < 0.8 ? CommonAttack : SpecialAttack);
            return;
        }
        else if (distance < 32 * 32 || navigationFinished)
        {
            EnterState(IDLE);
            return;
        }

        pathFindManager.Follow();
    }

    private void EnterCommonAttack()
    {
        sfxAttack.Play();
        EnterState(COMMON_ATTACK);
    }

    private async void CommonAttack()
    {
        await ToSignal(animationTree, "animation_finished");

        if (damageCreated) return;

        var position = GlobalPosition;
        var facingDirection = velocityManager.LastFacedDirection;

        new DamageFactory.HitBoxBuilder(position)
            .AddStatusEffectToPool(new StatusEffect.Info { Id = "stun", Chance = 0.2f })
            .SetShape(new RectangleShape2D { Size = new Vector2(HITBOX_SIZE, HITBOX_SIZE) })
            .SetShapeOffset(new Vector2(HITBOX_SIZE / 2, 0))
            .SetOwner(this)
            .SetRotation(facingDirection.Angle())
            .SetDamage(StatsManager.Damage)
            .Build();

        damageCreated = true;
    }

    private void LeaveCommonAttack()
    {
        attackTimer.Call(START_RANDOM);
        damageCreated = false;
    }

    private void EnterSpecialAttack()
    {
        sfxSpecial.Play();
        EnterState(SPECIAL_ATTACK);
    }

    private async void SpecialAttack()
    {
        await ToSignal(animationTree, "animation_finished");

        if (damageCreated) return;

        var position = GlobalPosition;
        var facingDirection = velocityManager.LastFacedDirection;

        new DamageFactory.HitBoxBuilder(position)
            .AddStatusEffectToPool(new StatusEffect.Info { Id = "stun", IsGuaranteed = true })
            .SetShape(new RectangleShape2D { Size = new Vector2(HITBOX_SIZE, HITBOX_SIZE) })
            .SetShapeOffset(new Vector2(HITBOX_SIZE / 2, 0))
            .SetOwner(this)
            .SetRotation(facingDirection.Angle())
            .SetDamage(StatsManager.Damage)
            .Build();

        damageCreated = true;
    }

    private void LeaveSpecialAttack()
    {
        attackTimer.Call(START_RANDOM);
        damageCreated = false;
    }

    private void EnterState(string state)
    {
        playback.Travel(state);
        UpdateBlendPosition();
    }

    private void OnAnimationFinished(StringName anim)
    {
        if (!anim.ToString().Contains("attack")) return;

        if (IsPlayerInRange)
        {
            StateMachine.ChangeState(TravelToPlayer);
        }
        else
        {
            StateMachine.ChangeState(Normal);
        }

        attackCooldownTimer.Start();
    }

    private void UpdateBlendPosition()
    {
        animationTree.Set("parameters/Move/blend_position", velocityManager.LastFacedDirection);
        animationTree.Set("parameters/Common Attack/blend_position", velocityManager.LastFacedDirection);
        animationTree.Set("parameters/Special Attack/blend_position", velocityManager.LastFacedDirection);
        animationTree.Set("parameters/Idle/blend_position", velocityManager.LastFacedDirection);
    }
}