using System;
using Game.Autoload;
using Game.Common;
using Game.Components;
using Game.Data;
using Godot;
using GodotUtilities;

namespace Game.UI.Overlays;

[Scene]
public partial class ModeSelect : Overlay
{
    [Node] private TextureButton closeButton;
    [Node] private Button storyModeButton;
    [Node] private Button frenzyModeButton;
    [Node] private ColorRect lockedTexture;
    [Node] private AudioStreamPlayer2D sFXClick;

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
        frenzyModeButton.Disabled = !SaveManager.Data.FrenzyModeUnlocked;
        lockedTexture.Visible = !SaveManager.Data.FrenzyModeUnlocked;

        frenzyModeButton.Pressed += () =>
        {
            sFXClick.Play();
            Close();
            GameManager.ChangeScene("res://Scenes/World/Levels/Noise.tscn");
        };

        storyModeButton.Pressed += () =>
        {
            sFXClick.Play();
            Close();

            var currentChapterBase64 = SaveManager.Data.CurrentChapter;
            var currentChapter = string.IsNullOrEmpty(currentChapterBase64)
                ? "res://Scenes/World/Levels/Story/Prologue/Prologue.tscn"
                : System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(currentChapterBase64));

            GameManager.ChangeScene(currentChapter);
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