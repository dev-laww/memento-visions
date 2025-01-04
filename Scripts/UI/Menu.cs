using Game.UI;
using Godot;
using GodotUtilities;

namespace Game;

[Scene]
public partial class Menu : Overlay
{
    [Node] private TextureButton closeButton;
    [Node] private Button resumeButton;
    [Node] private Button quitButton;
    private Tween tween;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        closeButton.Pressed += Close;
        resumeButton.Pressed += Close;
        quitButton.Pressed += () => GetTree().Quit();
    }

    public override void Open()
    {
        base.Open();

        tween = CreateTween();

        tween.TweenProperty(Engine.Singleton, "time_scale", 0.5, 0.5);
        tween.SetParallel().TweenProperty(GetTree(), "paused", true, 20);
    }

    public override void Close()
    {
        base.Close();
        tween.KillIfValid();

        Engine.TimeScale = 1;
        GetTree().Paused = false;
    }
}