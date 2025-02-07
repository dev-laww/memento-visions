using Game.Components;
using Godot;
using GodotUtilities;

namespace Game.UI;

[Scene]
public partial class Start : Control
{
    [Node] private Button button;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready() => button.Pressed += () => GameManager.ChangeScene("res://Scenes/World/Bar.tscn");
}