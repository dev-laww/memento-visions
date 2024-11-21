using Game.Globals;
using Godot;
using GodotUtilities;

namespace Game.Components.Area;

// TODO: add animations for showing and hiding the interaction UI

[Tool]
[Scene]
public partial class Interaction : Area2D
{
    [Export(PropertyHint.MultilineText)]
    public string InteractionLabel
    {
        get => GetNodeOrNull<Label>("%Label").Text;
        set
        {
            var label = GetNodeOrNull<Label>("%Label");

            if (label == null) return;

            label.Text = value;
            label.NotifyPropertyListChanged();
        }
    }

    [Signal]
    public delegate void InteractedEventHandler();

    [Node]
    private Node2D interactionUI;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        BodyEntered += body => InteractionManager.Register(this);
        BodyExited += body =>
        {
            InteractionManager.Unregister(this);
            HideUI();
        };

        if (Engine.IsEditorHint()) return;

        interactionUI.Hide();
    }

    public void Interact() => EmitSignal(SignalName.Interacted);

    public void HideUI() => interactionUI.Hide();

    public void ShowUI() => interactionUI.Show();
}