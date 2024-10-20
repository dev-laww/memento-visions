using System.Collections.Generic;
using System.Linq;
using Game.Globals;
using Godot;
using GodotUtilities;

namespace Game.Components.Area;

[Tool]
[Scene]
public partial class Interaction : Area2D
{
    [Export]
    public string InteractionLabel = "Interact";
    
    [Signal]
    public delegate void InteractedEventHandler();
    
    public Marker2D Marker { get; private set; }
    public override void _Notification(int what) {
        if (what != NotificationSceneInstantiated) return;
        
        WireNodes();
    }

    public override void _Ready()
    {
        BodyEntered += body => InteractionManager.Register(this);
        BodyExited += body => InteractionManager.Unregister(this);
        Marker = GetChildren().OfType<Marker2D>().First();
    }
    
    public void Interact() => EmitSignal(SignalName.Interacted);

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        var marker = GetChildren().OfType<Marker2D>().FirstOrDefault();
        
        if (marker == null)
            warnings.Add("Marker2D not found.");
        
        return warnings.ToArray();
    }
}

