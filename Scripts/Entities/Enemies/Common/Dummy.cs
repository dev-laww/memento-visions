using Game.Components.Managers;
using Game.Effects.HealthNumbers;
using Game.Entities;
using Game.Utils.Battle;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game;

[Scene]
public partial class Dummy : Entity
{
    [Node] private AnimationPlayer animationPlayer;
    [Node] private HealthNumberManager healthNumberManager;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        StatsManager.AttackReceived += OnAttackReceived;
    }

    private void OnAttackReceived(Attack attack)
    {
        animationPlayer.Stop();

        animationPlayer.Play(attack.Critical ? "crit" : "hit");
    }
}