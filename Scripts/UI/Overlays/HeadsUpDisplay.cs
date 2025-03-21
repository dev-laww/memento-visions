using System.Linq;
using Game.Common;
using Game.Components;
using Game.Entities;
using Game.UI.Common;
using Game.Utils.Battle;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.UI.Overlays;

[Scene]
public partial class HeadsUpDisplay : Overlay
{
    private const string HIT = "hit";

    [Node] private HealthBar healthBar;
    [Node] private HBoxContainer dashContainer;
    [Node] private TextureProgressBar dashIndicator;
    [Node] private GpuParticles2D healthParticlesForeground;
    [Node] private GpuParticles2D healthParticlesBackground;
    [Node] private TextureRect healthGlow;
    [Node] private AnimationPlayer animationPlayer;

    private Player player;

    private Tween healTween;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        player = this.GetPlayer();

        if (player == null)
        {
            Log.Warn("Player not found in scene.");
            return;
        }

        healthBar.Initialize(player.StatsManager);

        healthParticlesBackground.Emitting = false;
        healthParticlesForeground.Emitting = false;
        healthGlow.Modulate = Colors.Transparent;


        player.StatsManager.StatIncreased += OnStatIncreased;
        player.StatsManager.AttackReceived += OnAttackReceived;
        player.StatsManager.LevelUp += OnLevelUp;
        player.VelocityManager.Dashed += OnDash;
        dashIndicator.Value = 100;

        for (var i = 1; i < player.VelocityManager.TimesCanDash; i++)
        {
            dashContainer.AddChild(dashIndicator.Duplicate());
        }
    }

    private void OnDash(Vector2 _)
    {
        var indicators = dashContainer.GetChildrenOfType<DashIndicator>().Where(x => !x.Running).ToArray();

        if (indicators.Length == 0) return;

        var indicator = indicators.Last();

        indicator.Start(player.VelocityManager.DashCoolDown);
    }

    private void OnStatIncreased(float _, StatsType type)
    {
        if (type != StatsType.Health) return;

        healTween?.KillIfValid();

        healthParticlesForeground.Emitting = true;
        healthParticlesBackground.Emitting = true;

        healTween = CreateTween();

        healTween.TweenProperty(healthGlow, "modulate", Colors.White, 0.25f);
        healTween.TweenInterval(0.25f);
        healTween.TweenCallback(Callable.From(() =>
        {
            healthParticlesForeground.Emitting = false;
            healthParticlesBackground.Emitting = false;
        }));
        healTween.TweenProperty(healthGlow, "modulate", Colors.Transparent, 0.25f);
    }

    private void OnAttackReceived(Attack attack)
    {
        if (attack.Damage <= player.StatsManager.Defense) return;

        animationPlayer.Play(HIT);
    }


    private void OnLevelUp(float _)
    {
        healthBar.Initialize(player.StatsManager);
    }
}