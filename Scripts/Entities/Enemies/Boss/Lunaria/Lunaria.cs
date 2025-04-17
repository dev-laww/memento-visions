using Game.Autoload;
using Game.Common.Extensions;
using Game.Components;
using Game.Data;
using Game.Utils;
using Game.Utils.Battle;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.Entities;

[Scene]
public partial class Lunaria : Enemy
{
    private const string START_RANDOM = "start_random";
    private const string COMMON_ATTACK = "common_attack";
    private const string SPECIAL_ATTACK_1 = "special_attack_1";
    private const string SPECIAL_ATTACK_2 = "special_attack_2";
    private const int MAX_SPAWNED_CLOUDS = 3;
    private const float DISTANCE_TO_PLAYER = 16f;

    [Node] private AnimationTree animationTree;
    [Node] private VelocityManager velocityManager;
    [Node] private PathFindManager pathFindManager;
    [Node] private Timer commonAttackTimer;
    [Node] private Timer specialAttackTimer1;
    [Node] private Timer specialAttackTimer2;
    [Node] private Timer blinkTimer;
    [Node] private ResourcePreloader resourcePreloader;

    private AnimationNodeStateMachinePlayback playback;
    private int spawnedClouds;

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated)
        {
            WireNodes();
        }
    }

    public override void OnReady()
    {
        playback = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");

        StateMachine.AddStates(Normal, EnterNormal, ExitNormal);
        StateMachine.AddStates(TravelToPlayer, EnterTravelToPlayer);
        StateMachine.AddStates(CommonAttack, EnterCommonAttack, ExitCommonAttack);
        StateMachine.AddStates(MoonFlare, EnterMoonFlare);
        StateMachine.AddStates(SpawnVineTrap, EnterSpawnVinesTrap);
        StateMachine.SetInitialState(Normal);
    }

    public override void OnProcess(double delta)
    {
        velocityManager.ApplyMovement();
        UpdateBlendPositions();
    }

    #region States

    private void Normal()
    {
        var playerPosition = this.GetPlayer()?.GlobalPosition ?? GlobalPosition;
        pathFindManager.SetTargetPosition(playerPosition);

        if (GlobalPosition.DistanceSquaredTo(playerPosition) > DISTANCE_TO_PLAYER * DISTANCE_TO_PLAYER)
            pathFindManager.Follow();
        else
            velocityManager.Decelerate();

        if (commonAttackTimer.IsStopped())
            StateMachine.ChangeState(TravelToPlayer);

        if (specialAttackTimer1.IsStopped())
            StateMachine.ChangeState(MoonFlare);

        if (specialAttackTimer2.IsStopped())
            StateMachine.ChangeState(SpawnVineTrap);
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

        if (!pathFindManager.NavigationAgent2D.IsNavigationFinished() &&
            distanceToTarget > DISTANCE_TO_PLAYER * DISTANCE_TO_PLAYER) return;

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
        if (playback == null) return;

        playback.Travel(COMMON_ATTACK);

        if (StatsManager == null) return;

        var healthRegen = new HealthRegen
        {
            Id = "lunaria_heal",
            StatusEffectName = "Lunar Healing",
            Duration = 3.0f,
            HealthRegenPerTick = 0.08f * StatsManager.MaxHealth,
            TickInterval = 1f,
        };

        StatsManager.AddStatusEffect(healthRegen);
    }

    private void ExitCommonAttack()
    {
        commonAttackTimer.Call(START_RANDOM);
    }

    private async void MoonFlare()
    {
        await ToSignal(animationTree, "animation_finished");
        specialAttackTimer1.Call(START_RANDOM);
        StateMachine.ChangeState(Normal);
    }

    private void EnterMoonFlare()
    {
        playback.Travel(SPECIAL_ATTACK_1);
    }

    private async void SpawnVineTrap()
    {
        await ToSignal(animationTree, "animation_finished");
        specialAttackTimer2.Call(START_RANDOM);
        StateMachine.ChangeState(Normal);
    }

    private void EnterSpawnVinesTrap()
    {
        playback.Travel(SPECIAL_ATTACK_2);
        var vines = resourcePreloader.InstanceSceneOrNull<Vines>();
        GetTree().Root.AddChild(vines);
    }

    #endregion

    #region Utility

    private void UpdateBlendPositions()
    {
        animationTree.Set("parameters/move/blend_position", velocityManager.LastFacedDirection);
    }

    #endregion
}
