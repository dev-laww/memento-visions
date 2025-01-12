using Godot;
using GodotUtilities;

namespace Game.UI.Overlays;

[Scene]
public partial class HeadsUpDisplay : Control
{
    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }
}