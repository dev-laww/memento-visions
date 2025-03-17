using Game.Entities;
using Game.Utils.Battle;
using Godot;
using GodotUtilities;

namespace Game.Components;

[Tool]
[Scene]
public partial class CircleDamage : Damage
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

    public override void _Ready()
    {
        collisionShape2D.Disabled = true;
    }

    public override void SetOwner(Entity owner)
    {
        hitBox.HitboxOwner = owner;
    }

    public override void SetDamage(float damage)
    {
        hitBox.Damage = damage;
    }

    public override void SetDuration(float duration)
    {
        timer.WaitTime = duration;
    }

    public override void SetType(Attack.Type type)
    {
        hitBox.Type = type;
    }

    public override void SetRadius(float radius)
    {
        Radius = radius;
    }

    public override void Start(TelegraphCanvas canvas)
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

