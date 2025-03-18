using Godot;
using GodotUtilities;

namespace Game.Entities;

[Scene]
public partial class Tikbalang : Entity
{
    public override void _Notification(int what) {
        if (what != NotificationSceneInstantiated) return;
        
        WireNodes();
    }
}

