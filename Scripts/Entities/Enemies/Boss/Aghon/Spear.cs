using Godot;
using GodotUtilities;

namespace Game;

[Scene]
public partial class Spear : Node2D
{
    public override void _Notification(int what) {
        if (what != NotificationSceneInstantiated) return;
        
        WireNodes();
    }
}

