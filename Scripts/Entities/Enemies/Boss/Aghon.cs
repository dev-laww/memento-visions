using Game.Common.Extensions;
using Game.Components;
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
        pathFindManager.SetTargetPosition(this.GetPlayer()?.GlobalPosition ?? GlobalPosition);
        pathFindManager.Follow();

        if (commonAttackTimer.IsStopped())
        {
            StateMachine.ChangeState(TravelToPlayer);
        }
    }

    private void EnterNormal()
    {
        commonAttackTimer.Resume();
    }

    private void ExitNormal()
    {
        commonAttackTimer.Pause();
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
            .SetDamage(StatsManager.Damage)
            .SetDelay(.3f)
            .SetShape(new CircleShape2D { Radius = 40 })
            .SetOwner(this)
            .Build();

        new DamageFactory.HitBoxBuilder(GlobalPosition)
            .SetDamage(StatsManager.Damage)
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
    #endregion

    #region Utility
    private void UpdateBlendPositions()
    {
        animationTree.Set("parameters/move/blend_position", velocityManager.LastFacedDirection);
    }
    #endregion
}

