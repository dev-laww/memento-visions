using Godot;
using GodotUtilities;

namespace Game.Components;

[Scene]
public partial class LineDamage : Node2D
{
    [Node] private Timer timer;

    [Node] private HitBox hitBox;
    [Node] private CollisionShape2D collisionShape2D;

    [Export] private Vector2 endPosition;

    private LineTelegraph telegraph;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }
}

