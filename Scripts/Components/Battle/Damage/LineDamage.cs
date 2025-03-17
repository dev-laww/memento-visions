using Godot;
using GodotUtilities;

namespace Game.Components;

[Scene]
public partial class LineDamage : Node2D
{
    [Node] private Timer timer;
    [Node] public HitBox HitBox;
    [Node] private CollisionShape2D collisionShape2D;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        collisionShape2D.Disabled = true;

        timer.Timeout += OnTimerTimeout;
    }

    public void Start(Vector2 start, Vector2 end, float duration = -1)
    {
        GlobalPosition = start;

        var trajectory = end - start;
        var length = trajectory.Length();
        HitBox.Rotation = trajectory.Angle();

        var shape = (RectangleShape2D)collisionShape2D.Shape;

        shape.Size = new Vector2(length, 16);

        collisionShape2D.Position = new Vector2(length / 2, 0);
        timer.Start(duration);
    }

    public void OnTimerTimeout()
    {
        collisionShape2D.Disabled = false;

        GetTree().CreateTimer(0.1f).Timeout += QueueFree;
    }
}

