using Game.Entities;
using Game.Utils.Battle;
using Godot;
using GodotUtilities;

namespace Game.Components;

[Tool]
[Scene]
public partial class CircleDamage : Node2D
{
    [Node] private HitBox hitBox;
    [Node] private CollisionShape2D collisionShape2D;
    [Node] private Timer timer;

    private CircleTelegraph telegraph;

    [Export]
    private float Radius
    {
        get => ((CircleShape2D)GetNode<CollisionShape2D>("%CollisionShape2D").Shape).Radius;
        set
        {
            if (!IsNodeReady()) return;

            var shape = (CircleShape2D)GetNode<CollisionShape2D>("%CollisionShape2D")?.Shape;

            shape.Radius = value;
            shape.NotifyPropertyListChanged();
        }
    }

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }
}

