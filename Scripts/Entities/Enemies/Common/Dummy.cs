using Game.Effects.HealthNumbers;
using Game.Utils.Battle;
using Godot;
using GodotUtilities;

namespace Game.Entities.Enemies.Common;

[Scene]
public partial class Dummy : Enemy
{
    [Node] private AnimationPlayer animationPlayer;
    [Node] private HealthNumberManager healthNumberManager;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void OnReady()
    {
        StatsManager.AttackReceived += OnAttackReceived;
    }

    protected override void Die(DeathInfo info) { }

    private void OnAttackReceived(Attack attack)
    {
        animationPlayer.Stop();

        animationPlayer.Play(attack.Critical ? "crit" : "hit");
    }
}