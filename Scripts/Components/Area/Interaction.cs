using Game.Globals;
using Godot;
using GodotUtilities;

namespace Game.Components.Area;

public enum InteractionMode
{
    Normal,
    Deferred,
}

[Scene]
[Tool]
public partial class Interaction : Area2D
{
    [Signal]
    public delegate void InteractedEventHandler();
    
    public Callable Interact { private get; set; } = Callable.From(() => GD.Print("Interacted"));
    
    public override void _Notification(int what) {
        if (what != NotificationSceneInstantiated) return;
        
        WireNodes();
    }

    public override void _Ready()
    {
        BodyEntered += body => InteractionManager.Register(this);
        BodyExited += body => InteractionManager.Unregister(this);
    }

    public void InteractWith(InteractionMode mode = InteractionMode.Normal) {
        EmitSignal(SignalName.Interacted);

        if (mode == InteractionMode.Deferred)
        {
            Interact.CallDeferred();
            return;
        }
        
        Interact.Call();
    }
}

