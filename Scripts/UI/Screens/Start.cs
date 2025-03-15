using Game.Components;
using Godot;
using GodotUtilities;

namespace Game.UI.Screens;

[Scene]
public partial class Start : CanvasLayer
{
    [Node] private Button button;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready() => button.Pressed += () => GameManager.ChangeScene("res://Scenes/World/Bar.tscn");
}