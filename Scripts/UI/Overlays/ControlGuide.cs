using Godot;
using System;
using GodotUtilities;

namespace Game.UI.Overlays;

[Scene]
public partial class ControlGuide : Overlay
{
    [Node] private TextureButton closeButton;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        closeButton.Pressed += Close;
        
    }
}
