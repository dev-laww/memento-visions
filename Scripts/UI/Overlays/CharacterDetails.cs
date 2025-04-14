using Game.Components;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.UI.Overlays;

[Scene]
public partial class CharacterDetails : Overlay
{
    [Node] private TextureButton closeButton;
    [Node] private Label level;
    [Node] private Label experience;
    [Node] private Label maxHealth;
    [Node] private Label damage;
    [Node] private Label speed;
    [Node] private Label defense;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        closeButton.Pressed += Close;

        if (this.GetPlayer() is not { } player) return;

        var statsManager = player.StatsManager;
        var requiredExperience = StatsManager.CalculateRequiredExperience(statsManager.Level + 1);

        level.Text = Mathf.RoundToInt(statsManager.Level).ToString();
        experience.Text = $"{Mathf.RoundToInt(statsManager.Experience)} / {Mathf.RoundToInt(requiredExperience)}";
        maxHealth.Text = Mathf.RoundToInt(statsManager.MaxHealth).ToString();
        damage.Text = Mathf.RoundToInt(statsManager.Damage).ToString();
        speed.Text = Mathf.RoundToInt(statsManager.Speed).ToString();
        defense.Text = Mathf.RoundToInt(statsManager.Defense).ToString();
    }
}

