using Game.Entities;
using Game.UI.Common;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.UI.Overlays;

[Scene]
public partial class HeadsUpDisplay : Overlay
{
    [Node] private HealthBar healthBar;

    private Player player;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        player = this.GetPlayer();
        healthBar.Initialize(player.StatsManager);

        player.StatsManager.LevelUp += OnLevelUp;
    }

    private void OnLevelUp(float _)
    {
        healthBar.Initialize(player.StatsManager);
    }
}