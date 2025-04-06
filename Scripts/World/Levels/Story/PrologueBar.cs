using Godot;
using Game.Components;
using GodotUtilities;

namespace Game.World;

[Scene]
public partial class PrologueBar : Node2D
{
    public bool isInteracted = false;
    [Node] private TransitionArea TransitionArea;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        base._Ready();
        TransitionArea.Monitoring = false;
    }

    public void EnableTransitionArea()
    {
        TransitionArea.Monitoring = true;
    }
}