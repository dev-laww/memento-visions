using Game.Entities;
using Game.Utils.Battle;
using Godot;
using GodotUtilities;

namespace Game.Components;

[Scene]
public partial class LineDamage : Damage
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

    public override void _Ready()
    {
        collisionShape2D.Disabled = true;
    }

    public override void SetDamage(float damage)
    {
        hitBox.Damage = damage;
    }

    public override void SetDuration(float duration)
    {
        timer.WaitTime = duration;
    }

    public override void SetOwner(Entity owner)
    {
        hitBox.HitboxOwner = owner;
    }

    public override void SetType(Attack.Type type)
    {
        hitBox.Type = type;
    }

    public void SetTargetPosition(Vector2 position)
    {
        endPosition = position;

        var length = (position - GlobalPosition).Length();
        var angle = GlobalPosition.AngleToPoint(position);

        GlobalRotation = angle;

        collisionShape2D.Shape = new RectangleShape2D { Size = new Vector2(length, 16 * Scale.Y) };
        collisionShape2D.Position = new Vector2(length / 2, 0);
    }

    public override void Start(TelegraphCanvas canvas)
    {
        telegraph = canvas.CreateLineTelegraph(endPosition);
        telegraph.Finished += OnTelegraphFinished;

        GameManager.CurrentScene.AddChild(this);

        timer.Start();
    }

    private void OnTimerTimeout()
    {
        collisionShape2D.Disabled = false;
        telegraph.End();
    }

    private void OnTelegraphFinished()
    {
        QueueFree();
    }
}

