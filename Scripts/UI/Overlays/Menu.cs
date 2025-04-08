using Game.UI;
using Godot;
using GodotUtilities;

namespace Game.UI.Overlays;

[Scene]
public partial class Menu : Overlay
{
    [Node] private TextureButton closeButton;
    [Node] private Button resumeButton;
    [Node] private Button quitButton;
    [Node] private AudioStreamPlayer2D sfxClose;
    [Node] private AudioStreamPlayer2D sfxOpen;
    [Node] private AudioStreamPlayer2D sfxClick;
    private Tween tween;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        sfxOpen.Play();
        closeButton.Pressed += Close;
        resumeButton.Pressed += Close;
        quitButton.Pressed += () => GetTree().Quit();
        
    }

    public override void Close()
    {
        base.Close();
        sfxClose.Play();
        Engine.TimeScale = 1;
        GetTree().Paused = false;
    }
}