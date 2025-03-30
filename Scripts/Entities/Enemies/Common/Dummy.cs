using Game.Components;
using Game.Utils.Battle;
using Godot;
using GodotUtilities;

namespace Game.Entities;

[Scene]
public partial class Dummy : Enemy
{
    [Node] private AnimationPlayer animationPlayer;
    [Node] private AnimatedSprite2D smoothAnimatedSprite2d;
    [Node] private HealthNumberManager healthNumberManager;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void OnReady()
    {
        StatsManager.AttackReceived += OnAttackReceived;
        animationPlayer.AnimationFinished += _ => animationPlayer.Play("idle");
    }

    protected override void Die(DeathInfo info) { }

    private void OnAttackReceived(Attack attack)
    {
        var sourcePosition = attack.Source.GlobalPosition;

        var directionToSource = (sourcePosition - GlobalPosition).Normalized();
        smoothAnimatedSprite2d.FlipH = directionToSource.X > 0;

        animationPlayer.Stop();

        animationPlayer.Play(attack.Critical ? "crit" : "hit");
    }
}