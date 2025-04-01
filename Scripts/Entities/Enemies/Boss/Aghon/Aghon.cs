using Game.Common.Extensions;
using Game.Components;
using Game.Data;
using Game.Utils;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.Entities;

[Scene]
public partial class Aghon : Enemy
{
    private const string START_RANDOM = "start_random";
    private const string TRANSFORM = "transform";
    private const string COMMON_ATTACK = "common_attack";
    private const string SPECIAL_ATTACK_1 = "special_attack_1";
    private const string SPECIAL_ATTACK_2 = "special_attack_2";

    [Node] private AnimationTree animationTree;
    [Node] private VelocityManager velocityManager;
    [Node] private PathFindManager pathFindManager;
    [Node] private Timer commonAttackTimer;
    [Node] private Timer specialAttackTimer1;
    [Node] private Timer specialAttackTimer2;
    [Node] private ResourcePreloader resourcePreloader;


    private AnimationNodeStateMachinePlayback playback;
    private int phase = 1;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void OnReady()
    {
        playback = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");

        StateMachine.AddStates(Normal, EnterNormal, ExitNormal);
        StateMachine.AddStates(TravelToPlayer, EnterTravelToPlayer);
        StateMachine.AddStates(CommonAttack, EnterCommonAttack, ExitCommonAttack);
        StateMachine.AddStates(TransformToSecondPhase, EnterTransformToSecondPhase);
        StateMachine.AddStates(ShockWavePunch, EnterShockWavePunch, ExitShockWavePunch);
        StateMachine.AddStates(SpearThrow, EnterSpearThrow, ExitSpearThrow);

        StateMachine.SetInitialState(Normal);
    }

    public override void OnProcess(double delta)
    {
        velocityManager.ApplyMovement();
        UpdateBlendPositions();
    }

    protected override void Die(DeathInfo info)
    {
        phase++;

        if (phase > 2)
        {
            base.Die(info);
            return;
        }

        StateMachine.ChangeState(TransformToSecondPhase);
    }

    #region States
    private void Normal()
    {
        var playerPosition = this.GetPlayer()?.GlobalPosition ?? GlobalPosition;
        pathFindManager.SetTargetPosition(playerPosition);

        if (GlobalPosition.DistanceSquaredTo(playerPosition) > 64 * 64)
        {
            pathFindManager.Follow();
        }
        else
        {
            velocityManager.Decelerate();
        }

        if (commonAttackTimer.IsStopped())
        {
            StateMachine.ChangeState(TravelToPlayer);
        }

        if (specialAttackTimer1.IsStopped())
        {
            // StateMachine.ChangeState(phase == 1 ? ShockWavePunch : SecondPhaseSpecialAttack1);
            StateMachine.ChangeState(ShockWavePunch);
        }

        if (specialAttackTimer2.IsStopped())
        {
            // StateMachine.ChangeState(phase == 1 ? SpearThrow : SecondPhaseSpecialAttack2);
            StateMachine.ChangeState(SpearThrow);
        }
    }

    private void EnterNormal()
    {
        commonAttackTimer.Resume();
        specialAttackTimer1.Resume();
        specialAttackTimer2.Resume();
    }

    private void ExitNormal()
    {
        commonAttackTimer.Pause();
        specialAttackTimer1.Pause();
        specialAttackTimer2.Pause();
    }

    private void TravelToPlayer()
    {
        var distanceToTarget = GlobalPosition.DistanceSquaredTo(pathFindManager.GetTargetPosition());
        pathFindManager.Follow();

        if (!pathFindManager.NavigationAgent2D.IsNavigationFinished() && distanceToTarget > 32 * 32) return;

        StateMachine.ChangeState(CommonAttack);
    }

    private void EnterTravelToPlayer()
    {
        pathFindManager.SetTargetPosition(this.GetPlayer()?.GlobalPosition ?? GlobalPosition);
    }

    private async void CommonAttack()
    {
        await ToSignal(animationTree, "animation_finished");

        StateMachine.ChangeState(Normal);
    }

    private void EnterCommonAttack()
    {
        playback.Travel(COMMON_ATTACK);

        new DamageFactory.HitBoxBuilder(GlobalPosition)
            .AddStatusEffectToPool(new StatusEffect.Info { Id = "electrocute", IsGuaranteed = true })
            .SetDamage(StatsManager.Damage * .8f)
            .SetDelay(.3f)
            .SetShape(new CircleShape2D { Radius = 40 })
            .SetOwner(this)
            .Build();

        new DamageFactory.HitBoxBuilder(GlobalPosition)
            .AddStatusEffectToPool(new StatusEffect.Info { Id = "electrocute", IsGuaranteed = true })
            .SetDamage(StatsManager.Damage * .8f)
            .SetDelay(.7f)
            .SetShape(new CircleShape2D { Radius = 40 })
            .SetOwner(this)
            .Build();
    }

    private void ExitCommonAttack()
    {
        commonAttackTimer.Call(START_RANDOM);
    }

    private async void TransformToSecondPhase()
    {
        await ToSignal(animationTree, "animation_finished");
        await ToSignal(GetTree().CreateTimer(1f), "timeout");

        StateMachine.ChangeState(Normal);
        StatsManager.SetInvulnerable(false);
    }

    private void EnterTransformToSecondPhase()
    {
        playback.Travel(TRANSFORM);
        StatsManager.SetInvulnerable(true);

        StatsManager.Heal(100, StatsManager.ModifyMode.Percentage);
        StatsManager.IncreaseDamage(40, StatsManager.ModifyMode.Percentage);
        StatsManager.DecreaseDefense(100, StatsManager.ModifyMode.Percentage);
        StatsManager.ApplySpeedModifier("second_phase", .15f);
    }

    private async void ShockWavePunch()
    {
        await ToSignal(animationTree, "animation_finished");

        StateMachine.ChangeState(Normal);
    }

    private void EnterShockWavePunch()
    {
        playback.Travel(SPECIAL_ATTACK_1);

        var playerPosition = this.GetPlayer()?.GlobalPosition ?? GlobalPosition;
        var direction = (playerPosition - GlobalPosition).Normalized();
        var attackOrigin = GlobalPosition;
        var attackDestination = attackOrigin + (direction * ShockWave.ATTACK_LENGTH);

        var shockWave = resourcePreloader.InstanceSceneOrNull<ShockWave>();

        GetTree().Root.AddChild(shockWave);

        shockWave.Start(attackOrigin, attackDestination, this);
    }

    private void ExitShockWavePunch()
    {
        specialAttackTimer1.Call(START_RANDOM);
    }

    private async void SpearThrow()
    {
        await ToSignal(animationTree, "animation_finished");
        StateMachine.ChangeState(Normal);
    }

    private void EnterSpearThrow()
    {
        playback.Travel(SPECIAL_ATTACK_2);

        var spear = resourcePreloader.InstanceSceneOrNull<Spear>();

        GetTree().Root.AddChild(spear);
    }

    private void ExitSpearThrow()
    {
        specialAttackTimer2.Call(START_RANDOM);
    }
    #endregion

    #region Utility
    private void UpdateBlendPositions()
    {
        animationTree.Set("parameters/move/blend_position", velocityManager.LastFacedDirection);
    }
    #endregion
}

