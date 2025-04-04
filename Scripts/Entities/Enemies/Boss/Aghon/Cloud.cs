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
public partial class Cloud : Enemy
{
    private const int ZAP_COUNT = 3;
    private const float ZAP_RADIUS = 30f;

    [Node] private AnimationTree animationTree;
    [Node] private Timer zapTimer;
    [Node] private VelocityManager velocityManager;
    [Node] private PathFindManager pathFindManager;

    private AnimationNodeStateMachinePlayback playback;
    private int currentZapCount = 0;
    private bool damaged;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void OnReady()
    {
        playback = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");

        StateMachine.AddStates(Normal);
        StateMachine.AddStates(TravelToPlayer, EnterTravelToPlayer);
        StateMachine.AddStates(Zap, EnterZap);
        StateMachine.AddStates(Despawn, EnterDespawn);

        StateMachine.SetInitialState(Normal);
    }

    public override void OnProcess(double delta)
    {
        pathFindManager.Follow();
        velocityManager.ApplyMovement();
    }

    #region States
    private void Normal()
    {
        if (currentZapCount >= ZAP_COUNT)
        {
            StateMachine.ChangeState(Despawn);
        }

        if (!zapTimer.IsStopped()) return;

        StateMachine.ChangeState(TravelToPlayer);
    }

    private void TravelToPlayer()
    {
        if (!damaged && pathFindManager.NavigationAgent2D.IsNavigationFinished())
        {
            damaged = true;

            var aghon = GetTree().Root.GetFirstChildOrNull<Aghon>();

            new DamageFactory.HitBoxBuilder(GlobalPosition)
                // .SetDamage(69)
                // .SetOwner(this)
                .SetDamage((aghon?.StatsManager.Damage ?? StatsManager.Damage) * .6f)
                .AddStatusEffectToPool(new StatusEffect.Info { Id = "electrocute", IsGuaranteed = true })
                .SetOwner(aghon as Enemy ?? this)
                .SetDamageType(Attack.Type.Magical)
                .SetDelay(.2f)
                .SetShape(new CircleShape2D { Radius = ZAP_RADIUS })
                .Build();
        }

        if (!pathFindManager.NavigationAgent2D.IsNavigationFinished()) return;

        StatsManager.RemoveSpeedModifier("travel");
        StateMachine.ChangeState(Zap);
    }

    private void EnterTravelToPlayer()
    {
        damaged = false;
        var playerPosition = this.GetPlayer()?.GlobalPosition ?? GlobalPosition;
        pathFindManager.SetTargetPosition(playerPosition);
        StatsManager.ApplySpeedModifier("travel", 3f);
    }

    private async void Zap()
    {
        await ToSignal(animationTree, "animation_finished");

        zapTimer.Call("start_random");
        StateMachine.ChangeState(Normal);
    }

    private void EnterZap()
    {
        currentZapCount++;
        playback.Travel("zap");
    }

    private async void Despawn()
    {
        await ToSignal(animationTree, "animation_finished");

        QueueFree();
    }

    private void EnterDespawn()
    {
        playback.Travel("despawn");
    }
    #endregion
}

