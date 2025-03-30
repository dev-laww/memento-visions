using Game.Components;
using Godot;
using GodotUtilities;

namespace Game.UI.Overlays;


[Scene]
public partial class ModeSelect : Overlay
{
    [Node] private TextureButton closeButton;
    [Node] private Button storyModeButton;
    [Node] private Button frenzyModeButton;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        closeButton.Pressed += Close;

        storyModeButton.MouseEntered += () => OnMouseEntered(storyModeButton);
        storyModeButton.MouseExited += () => OnMouseExited(storyModeButton);
        frenzyModeButton.MouseEntered += () => OnMouseEntered(frenzyModeButton);
        frenzyModeButton.MouseExited += () => OnMouseExited(frenzyModeButton);

        frenzyModeButton.Pressed += () =>
        {
            Close();
            GameManager.ChangeScene("res://Scenes/World/Levels/Noise.tscn");
        };

        storyModeButton.Pressed += () =>
        {
            Close();
            GameManager.ChangeScene("res://Scenes/World/Levels/Story.tscn");
        };
    }

    public void OnMouseEntered(Node node)
    {
        var tween = CreateTween();

        tween.TweenProperty(node, "scale", new Vector2(1.05f, 1.05f), 0.1f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.In);
    }

    public void OnMouseExited(Node node)
    {
        var tween = CreateTween();

        tween.TweenProperty(node, "scale", new Vector2(1f, 1f), 0.1f)
            .SetTrans(Tween.TransitionType.Circ)
            .SetEase(Tween.EaseType.Out);
    }
}
