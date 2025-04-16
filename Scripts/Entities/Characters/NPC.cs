using Godot;
using System;
using Game.Entities;
using GodotUtilities;

namespace Game.Entities;

[Scene]
public partial class NPC : Entity
{
    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }
    
    public override void OnReady()
    {
        base.OnReady();
    }
    
}
