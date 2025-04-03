using System;
using Godot;
using GodotUtilities;

namespace Game.UI.Overlays;

[Tool]
[Scene]
public partial class CenterText : CanvasLayer
{
    public event Action AnimationFinished;

    [Export]
    public string Text
    {
        get => label.Text;
        set
        {
            label.Text = value;
            label.NotifyPropertyListChanged();
        }
    }

    [Node] private Label label;
    [Node] private AnimationPlayer animationPlayer;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        animationPlayer.AnimationFinished += _ => AnimationFinished?.Invoke();
    }

    public void Start()
    {
        animationPlayer.Play("animate");
    }

    public void End()
    {
        GD.Print("End");
        animationPlayer.PlayBackwards("animate");
    }
}

