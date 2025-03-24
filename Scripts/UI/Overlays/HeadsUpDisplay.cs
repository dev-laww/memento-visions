using System.Linq;
using Game.Autoload;
using Game.Common;
using Game.Components;
using Game.Data;
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
    [Node] private Panel quickUseSlot;
    [Node] private TextureRect quickUseIcon;
    [Node] private Label quickUseLabel;

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
        PlayerInventoryManager.Updated += OnInventoryUpdated;
        GameEvents.Instance.QuickUseSlotUpdated += UpdateQuickUseSlot;


        dashIndicator.Value = 100;

        for (var i = 1; i < player.VelocityManager.TimesCanDash; i++)
        {
            dashContainer.AddChild(dashIndicator.Duplicate());
        }

        var quickUseItem = PlayerInventoryManager.QuickSlotItem;
        var quickUseGroup = PlayerInventoryManager.GetItem(quickUseItem);

        UpdateQuickUseSlot(quickUseGroup);
    }

    public override void _Process(double delta)
    {
        if (player is null || !player.InputManager.IsActionPressed("quick_use")) return;

        var item = PlayerInventoryManager.QuickSlotItem;
        PlayerInventoryManager.UseItem(item);
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

    // TODO: Fix errors on quick use slot
    private void OnInventoryUpdated(ItemGroup group)
    {
        if (group.Item.Id != PlayerInventoryManager.QuickSlotItem?.Id) return;

        UpdateQuickUseSlot(group);
    }

    private void UpdateQuickUseSlot(ItemGroup group)
    {
        quickUseSlot.Visible = group is not null;

        if (group is null) return;

        quickUseIcon.Texture = group.Item.Icon;
        quickUseLabel.Text = group.Quantity > 999 ? "999+" : group.Quantity.ToString();
    }
}