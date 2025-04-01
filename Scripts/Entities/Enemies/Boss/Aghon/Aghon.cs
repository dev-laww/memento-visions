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
    private const int MAX_SPAWNED_CLOUDS = 3;

    [Node] private AnimationTree animationTree;
    [Node] private VelocityManager velocityManager;
    [Node] private PathFindManager pathFindManager;
    [Node] private Timer commonAttackTimer;
    [Node] private Timer specialAttackTimer1;
    [Node] private Timer specialAttackTimer2;
    [Node] private Timer blinkTimer;
    [Node] private ResourcePreloader resourcePreloader;


    private AnimationNodeStateMachinePlayback playback;
    private int phase = 1;
    private bool blinked;
    private int spawnedClouds;

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
        StateMachine.AddStates(ShockWavePunch, EnterShockWavePunch);
        StateMachine.AddStates(SpearThrow, EnterSpearThrow);
        StateMachine.AddStates(Blink, EnterBlink);
        StateMachine.AddStates(SpawnCloud, EnterSpawnCloud);

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
            StateMachine.ChangeState(phase == 1 ? ShockWavePunch : Blink);
        }

        if (specialAttackTimer2.IsStopped())
        {
            StateMachine.ChangeState(phase == 1 ? SpearThrow : SpawnCloud);
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
        StatsManager.ApplySpeedModifier("second_phase", .3f);
    }

    private async void ShockWavePunch()
    {
        await ToSignal(animationTree, "animation_finished");

        specialAttackTimer1.Call(START_RANDOM);
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

    private async void SpearThrow()
    {
        await ToSignal(animationTree, "animation_finished");
        specialAttackTimer2.Call(START_RANDOM);
        StateMachine.ChangeState(Normal);
    }

    private void EnterSpearThrow()
    {
        playback.Travel(SPECIAL_ATTACK_2);

        var spear = resourcePreloader.InstanceSceneOrNull<Spear>();

        GetTree().Root.AddChild(spear);
    }

    private async void Blink()
    {
        if (blinkTimer.IsStopped() && !blinked)
        {
            var playerPosition = this.GetPlayer()?.GlobalPosition ?? GlobalPosition;
            var targetPosition = MathUtil.RNG.RandDirection() * 32 + playerPosition;

            velocityManager.Teleport(targetPosition);

            new DamageFactory.HitBoxBuilder(GlobalPosition)
                .AddStatusEffectToPool(new StatusEffect.Info { Id = "electrocute", IsGuaranteed = true })
                .SetDamage(StatsManager.Damage)
                .SetDelay(.6f)
                .SetShape(new CircleShape2D { Radius = 60 })
                .SetOwner(this)
                .Build(); // spawn circular shockwave

            blinked = true;
        }

        await ToSignal(animationTree, "animation_finished");

        blinked = false;
        specialAttackTimer1.Call(START_RANDOM);
        StateMachine.ChangeState(Normal);
    }

    private void EnterBlink()
    {
        playback.Travel(SPECIAL_ATTACK_1);
        blinkTimer.Start();
    }

    private async void SpawnCloud()
    {
        await ToSignal(animationTree, "animation_finished");

        StateMachine.ChangeState(Normal);
        specialAttackTimer2.Call(START_RANDOM);
    }

    private void EnterSpawnCloud()
    {
        playback.Travel(SPECIAL_ATTACK_2);

        if (spawnedClouds >= MAX_SPAWNED_CLOUDS)
        {
            StateMachine.ChangeState(Normal);
            return;
        }

        var cloud = resourcePreloader.InstanceSceneOrNull<Cloud>();

        if (cloud == null) return;

        cloud.GlobalPosition = GlobalPosition;
        cloud.TreeExiting += () => spawnedClouds = Mathf.Clamp(spawnedClouds - 1, 0, MAX_SPAWNED_CLOUDS);

        GetTree().Root.AddChild(cloud);

        spawnedClouds++;
    }
    #endregion

    #region Utility
    private void UpdateBlendPositions()
    {
        animationTree.Set("parameters/move/blend_position", velocityManager.LastFacedDirection);
    }
    #endregion
}

