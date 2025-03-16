using Game.Components;
using Game.Entities;
using Godot;
using GodotUtilities;

namespace Game;

[Tool]
[Scene]
public partial class CircleDamage : Node2D
{
    [Node] private HitBox hitBox;
    [Node] private CollisionShape2D collisionShape2D;
    [Node] private Timer timer;

    private CircleTelegraph telegraph;

    [Export]
    private float WaitTime
    {
        get => (float)GetNode<Timer>("Timer").WaitTime;
        set
        {
            var timer = GetNode<Timer>("Timer");

            timer.WaitTime = value;
            timer.NotifyPropertyListChanged();
        }
    }

    [Export]
    private float Radius
    {
        get => ((CircleShape2D)GetNode<CollisionShape2D>("%CollisionShape2D").Shape).Radius;
        set
        {
            var shape = (CircleShape2D)GetNode<CollisionShape2D>("%CollisionShape2D").Shape;

            shape.Radius = value;
            shape.NotifyPropertyListChanged();
        }
    }

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        collisionShape2D.Disabled = true;
    }

    public void SetOwner(Entity owner)
    {
        hitBox.HitboxOwner = owner;
    }

    public void SetDamage(float damage)
    {
        hitBox.Damage = damage;
    }

    public void SetDuration(float duration)
    {
        timer.WaitTime = duration;
    }

    public void Start(TelegraphCanvas canvas)
    {
        timer.Start();

        telegraph = canvas.CreateCircleTelegraph(Radius);
    }

    private void OnTimerTimeout()
    {
        telegraph.QueueFree(); // TODO: Implement a fade out animation

        collisionShape2D.Disabled = false;

        QueueFree(); // TODO: Make queue free after fade out animation
    }
}

