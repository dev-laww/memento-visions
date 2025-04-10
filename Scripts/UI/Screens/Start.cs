using Game.Components;
using Godot;
using GodotUtilities;

namespace Game.UI.Screens;

[Scene]
public partial class Start : CanvasLayer
{
    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion)
            return;

        GameManager.ChangeScene("res://Scenes/World/Bar.tscn");
        GetViewport().SetInputAsHandled();
    }
}