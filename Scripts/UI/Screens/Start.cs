using System.Linq;
using Game.Globals;
using Godot;
using GodotUtilities;

namespace Game;

[Scene]
public partial class Start : Control
{
    [Node]
    private Button button;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;
        

        WireNodes();
    }

    public override void _Ready() => button.Pressed += () => GameManager.ChangeScene("res://Scenes/Bar.tscn");
}