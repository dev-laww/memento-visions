using Game.Components.Managers;
using Game.Effects.HealthNumbers;
using Game.Utils.Battle;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game;

[Scene]
public partial class Dummy : CharacterBody2D
{
    [Node]
    private StatsManager statsManager;

    [Node]
    private AnimationPlayer animationPlayer;

    [Node]
    private HealthNumberManager healthNumberManager;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        statsManager.AttackReceived += OnAttackReceived;
    }
    

    private void OnAttackReceived(float dmg, Attack.Type type, bool critical)
    {
        animationPlayer.Stop();

        if (critical)
            animationPlayer.Play("crit");
        else
            animationPlayer.Play("hit");
    }
}