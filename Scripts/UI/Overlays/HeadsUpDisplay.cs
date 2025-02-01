using Godot;
using GodotUtilities;

namespace Game.UI.Overlays;

// [Scene]
public partial class HeadsUpDisplay : Overlay
{
    // public override void _Notification(int what)
    // {
    //     if (what != NotificationSceneInstantiated) return;
    //
    //     WireNodes();
    // }

    public override void Close()
    {
        base.Close();

        MouseFilter = MouseFilterEnum.Ignore;
    }

    public override void Open()
    {
        base.Open();

        MouseFilter = MouseFilterEnum.Ignore;
    }
}