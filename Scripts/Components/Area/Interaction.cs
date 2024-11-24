using Game.Globals;
using Godot;
using GodotUtilities;

namespace Game.Components.Area;

// TODO: add animations for showing and hiding the interaction UI

[Tool]
[GlobalClass]
public partial class Interaction : Area2D
{
    [Export(PropertyHint.MultilineText)]
    public string InteractionLabel
    {
        get
        {
            var label = GetNodeOrNull<Label>("InteractionUI/HBoxContainer/Label");

            return label != null ? label.Text : string.Empty;
        }
        set
        {
            var label = GetNodeOrNull<Label>("InteractionUI/HBoxContainer/Label");

            if (label == null) return;

            label.Text = value;
            label.NotifyPropertyListChanged();
        }
    }

    private Node2D interactionUI => GetNode<Node2D>("InteractionUI");

    [Signal]
    public delegate void InteractedEventHandler();

    public override void _EnterTree()
    {
        if (GetNodeOrNull("InteractionUI") != null) return;

        var ui = new Node2D();
        ui.Name = "InteractionUI";

        var container = new HBoxContainer();
        var texture = new TextureRect();
        var label = new Label();

        container.AddChild(texture);
        container.AddChild(label);
        ui.AddChild(container);
        AddChild(ui);

        container.Name = nameof(HBoxContainer);
        texture.Name = nameof(TextureRect);
        texture.Texture = GD.Load<Texture2D>("res://assets/icons/f.png");
        label.Name = nameof(Label);
        label.Text = "Interact";
        label.Theme = GD.Load<Theme>("res://resources/theme.tres");
        ui.Scale = new Vector2(0.5f, 0.5f);

        container.SetVSizeFlags(Control.SizeFlags.ShrinkCenter);
        container.SetHSizeFlags(Control.SizeFlags.ShrinkCenter);
        container.SetAnchorsPreset(Control.LayoutPreset.Center);
        container.SetHGrowDirection(Control.GrowDirection.Both);
        container.SetVGrowDirection(Control.GrowDirection.Both);
        texture.SetVSizeFlags(Control.SizeFlags.ShrinkCenter);
        texture.SetHSizeFlags(Control.SizeFlags.ShrinkCenter);

        ui.SetDisplayFolded(true);
        ui.NotifyPropertyListChanged();
        container.NotifyPropertyListChanged();
        texture.NotifyPropertyListChanged();

        ui.SetOwner(GetTree().GetEditedSceneRoot());
        container.SetOwner(GetTree().GetEditedSceneRoot());
        texture.SetOwner(GetTree().GetEditedSceneRoot());
        label.SetOwner(GetTree().GetEditedSceneRoot());
    }

    public override void _Ready()
    {
        CollisionLayer = 1 << 4;
        CollisionMask = 1 << 2;
        NotifyPropertyListChanged();

        BodyEntered += body => InteractionManager.Register(this);
        BodyExited += body => InteractionManager.Unregister(this);

        if (Engine.IsEditorHint()) return;

        interactionUI.Hide();
    }

    public void Interact() => EmitSignal(SignalName.Interacted);

    public void HideUI() => interactionUI.Hide();

    public void ShowUI() => interactionUI.Show();
}