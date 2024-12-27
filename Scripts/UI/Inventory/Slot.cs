using Godot;
using GodotUtilities;

namespace Game;

[Scene]
public partial class Slot : Control
{
    [Node]
    private AnimationPlayer animationPlayer;

    [Node] public Button button;

    public bool IsSelected => animationPlayer.CurrentAnimation == "select";
    public bool IsOccupied { get; set; }

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        button.Pressed += Select;
        button.SetDefaultCursorShape(CursorShape.PointingHand);
    }

    private void Select()
    {
        GetTree().CallGroup("Slots", "Deselect");
        animationPlayer.Play("select");
    }

    private void Deselect() => animationPlayer.Play("RESET");
}