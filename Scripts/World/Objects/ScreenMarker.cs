using Godot;
using GodotUtilities;

namespace Game;

[Tool]
[Scene]
public partial class ScreenMarker : Node2D
{
    private readonly Texture2D red = ResourceLoader.Load<Texture2D>("res://assets/world/arrow-red.png");
    private readonly Texture2D normal = ResourceLoader.Load<Texture2D>("res://assets/world/arrow.png");

    [Export] public bool HideWhenInBounds = true;
    [Export]
    public bool IsRed
    {
        get => smoothSprite2D.Texture.ResourcePath == red.ResourcePath;
        set
        {
            smoothSprite2D.Texture = value ? red : normal;
            smoothSprite2D.NotifyPropertyListChanged();
        }
    }

    [Node] private Sprite2D smoothSprite2D;

    private Vector2 originalPosition;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        originalPosition = GlobalPosition;
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint()) return;

        var canvas = GetCanvasTransform();
        var topLeft = -canvas.Origin / canvas.Scale;
        var size = GetViewportRect().Size / canvas.Scale;

        var bounds = new Rect2(topLeft, size);

        SetMarkerPosition(bounds);
    }

    private void SetMarkerPosition(Rect2 bounds)
    {
        var targetPosition = originalPosition;
        var isOutsideBounds = !bounds.HasPoint(targetPosition);
        var targetMarkerPosition = isOutsideBounds
            ? new Vector2(
                Mathf.Clamp(targetPosition.X, bounds.Position.X, bounds.End.X),
                Mathf.Clamp(targetPosition.Y, bounds.Position.Y, bounds.End.Y)
            )
            : originalPosition;

        Visible = !HideWhenInBounds || isOutsideBounds;
        GlobalPosition = GlobalPosition.Lerp(targetMarkerPosition, (float)GetProcessDeltaTime() * 10f);

        var targetAngle = isOutsideBounds ? (bounds.GetCenter() - targetPosition).Angle() + Mathf.Pi / 2 : 0;
        GlobalRotation = Mathf.LerpAngle(GlobalRotation, targetAngle, (float)GetProcessDeltaTime() * 5f);
    }
}

