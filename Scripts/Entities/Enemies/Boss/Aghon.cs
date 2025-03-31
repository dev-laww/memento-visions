using Godot;
using GodotUtilities;

namespace Game.Entities;

[Scene]
public partial class Aghon : Entity
{
    private int phase = 1;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }
}

