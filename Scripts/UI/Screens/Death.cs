using Game.Autoload;
using Game.Components;
using Godot;
using GodotUtilities;

namespace Game.UI.Screens;

[Scene]
public partial class Death : CanvasLayer
{
    private const string LOBBY = "res://Scenes/World/Bar.tscn";

    [Node] private ColorRect colorRect;
    [Node] private Button respawnButton;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        colorRect.Modulate = new Color(1, 1, 1, 0);
        respawnButton.Pressed += OnRespawnButtonPressed;

        var tween = CreateTween();

        tween.TweenMethod(Callable.From((Vector2 zoom) => GameCamera.SetZoom(zoom)), Vector2.One, new Vector2(1.1f, 1.1f), 0.5f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.In);

        tween.TweenProperty(colorRect, "modulate", new Color(1, 1, 1, 1), 1f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
    }

    private void OnRespawnButtonPressed()
    {
        GameManager.ChangeScene(LOBBY);
        GameCamera.SetZoom(Vector2.One);
        QueueFree();
    }
}
