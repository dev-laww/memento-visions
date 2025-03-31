using Godot;
using GodotUtilities;

namespace Game.UI.Common;

[Tool]
[Scene]
public partial class BossHealthBar : VBoxContainer
{
    [Export]
    public string BossName
    {
        get => IsNodeReady() ? label?.Text : string.Empty;
        set
        {
            if (!IsNodeReady() || label is null) return;

            label.Text = value;
            label.NotifyPropertyListChanged();
        }
    }

    [Node] public HealthBar HealthBar;
    [Node] private Label label;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }
}

