using System.Linq;
using Game.Components;
using Godot;
using Godot.Collections;
using GodotUtilities;

namespace Game.World.Puzzle;

[Scene]
public partial class Lever : Sprite2D
{
    [Node] private Interaction Interaction;
    [Export] public Array<Torch> Torches = new();
    private LeverManager Manager;
    private bool _switchState;

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated && !Engine.IsEditorHint())
            WireNodes();
    }

    public override void _Ready()
    {
        base._Ready();
        Interaction.Interacted += OnInteract;
        Manager = GetParent<LeverManager>();
    }
    

    private void OnInteract()
    {
        _switchState = !_switchState;
        Frame = _switchState ? 1 : 3;
        
        // Use the new method name and pass all affected torches at once
        Manager?.OnLeverToggled(Torches.ToArray(), _switchState);
    }
}