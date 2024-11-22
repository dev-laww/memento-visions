using Game.Battle;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.Effects.HealthNumbers;

[Tool]
[Scene]
public partial class DamageNumber : Node2D
{
    [Export(PropertyHint.MultilineText)]
    public string Text
    {
        get => label.Text;
        set
        {
            if (label == null) return;

            label.Text = value;

            if (value == string.Empty)
            {
                label.Size = new Vector2(1, 8);
                label.Position = new Vector2(0, -4);
            }

            label.NotifyPropertyListChanged();
        }
    }

    [Export]
    public Attack.Type DamageType
    {
        get => damageType;
        set
        {
            if (label == null) return;

            damageType = value;
            label.LabelSettings = value switch
            {
                Attack.Type.Physical => resourcePreloader?.GetResource<LabelSettings>("physical"),
                Attack.Type.Magical => resourcePreloader?.GetResource<LabelSettings>("magical"),
                _ => resourcePreloader?.GetResource<LabelSettings>("damage")
            };
            label.NotifyPropertyListChanged();
        }
    }

    [Node]
    private AnimationPlayer animationPlayer;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    private Attack.Type damageType = Attack.Type.Physical;
    private Label label => GetNodeOrNull<Label>("Label");
    private ResourcePreloader resourcePreloader => GetNodeOrNull<ResourcePreloader>("ResourcePreloader");

    public void Animate()
    {
        animationPlayer.Stop();
        animationPlayer.Play("entry");
    }

    public void Critical()
    {
        animationPlayer.Stop();
        animationPlayer.Play("critical");
    }

    public SignalAwaiter Exit()
    {
        animationPlayer.Play("exit");

        return ToSignal(animationPlayer, "animation_finished");
    }
}