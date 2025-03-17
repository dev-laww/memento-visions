using Game.Components;
using Game.Entities;
using Game.Utils.Battle;
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
            var timer = GetNodeOrNull<Timer>("Timer");

            if (timer == null) return;

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
            var shape = (CircleShape2D)GetNodeOrNull<CollisionShape2D>("%CollisionShape2D")?.Shape;

            if (shape == null) return;

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

    public void SetType(Attack.Type type)
    {
        hitBox.Type = type;
    }

    public void SetRadius(float radius)
    {
        Radius = radius;
    }

    public void Start(TelegraphCanvas canvas)
    {
        telegraph = canvas.CreateCircleTelegraph(Radius);
        telegraph.Finished += OnTelegraphFinished;

        // AddChild(telegraph);
        GameManager.CurrentScene.AddChild(this);

        timer.Start();
    }

    private void OnTimerTimeout()
    {
        telegraph.End();
        collisionShape2D.Disabled = false;
    }

    private void OnTelegraphFinished()
    {
        QueueFree();
    }
}

