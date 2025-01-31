using Game.Common;
using Game.Common.Interfaces;
using Game.Globals;
using Game.UI.Common;
using Godot;
using GodotUtilities;

namespace Game.Components.Area;

// TODO: add animations for showing and hiding the interaction UI

[Tool]
[Scene]
[GlobalClass]
public partial class Interaction : Area2D, IInteractable
{
    [Export(PropertyHint.MultilineText)]
    public string InteractionLabel
    {
        get => InteractionUI?.Text ?? string.Empty;
        set
        {
            interactionLabel = value;

            if (InteractionUI == null) return;

            InteractionUI.Text = value;
        }
    }

    [Signal] public delegate void InteractedEventHandler();

    private InteractionUI InteractionUI => GetNodeOrNull<InteractionUI>("%InteractionUI");

    private string interactionLabel;

    public override void _EnterTree()
    {
        if (GetNodeOrNull("Node2D") != null) return;

        var node = new Node2D { Name = "Node2D" };

        var collision = new CollisionShape2D
        {
            Name = "CollisionShape2D",
            DebugColor = new Color(0.88f, 0.525f, 0.898f, 0.42f)
        };
        var scene = ResourceLoader.Load<PackedScene>("res://Scenes/UI/Common/InteractionUI.tscn");
        var ui = scene.Instantiate<InteractionUI>();
        ui.UniqueNameInOwner = true;

        ui.SetOwner(GetTree().GetEditedSceneRoot());
        node.SetOwner(GetTree().GetEditedSceneRoot());
        node.AddChild(ui);

        AddChild(collision);
        AddChild(node);

        node.SetDisplayFolded(true);
        collision.NotifyPropertyListChanged();
        ui.NotifyPropertyListChanged();
        node.NotifyPropertyListChanged();

        collision.SetOwner(GetTree().GetEditedSceneRoot());
        ui.SetOwner(GetTree().GetEditedSceneRoot());
        node.SetOwner(GetTree().GetEditedSceneRoot());

        Log.Debug(ui.Owner.Name);
    }

    public override void _Ready()
    {
        CollisionLayer = 1 << 4;
        CollisionMask = 1 << 2;
        NotifyPropertyListChanged();

        if (InteractionUI == null) return;

        InteractionUI.Text = interactionLabel;

        if (Engine.IsEditorHint()) return;

        BodyEntered += _ => InteractionManager.Register(this);
        BodyExited += _ => InteractionManager.Unregister(this);

        InteractionUI.Hide();
    }

    public Vector2 InteractionPosition => GlobalPosition;

    public void Interact() => EmitSignal(SignalName.Interacted);

    public void HideUI() => InteractionUI?.Hide();

    public void ShowUI() => InteractionUI?.Show();
}