using Game.Common.Extensions;
using Game.Common.Interfaces;
using Game.Globals;
using Game.UI.Common;
using Godot;

namespace Game.Components.Area;

[Tool]
[GlobalClass]
public partial class Interaction : Area2D, IInteractable
{
    [Export]
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

    private InteractionUI InteractionUI => GetNodeOrNull<InteractionUI>("Node2D/InteractionUI");

    private string interactionLabel;

    public override void _EnterTree()
    {
        this.AddInteractionUI();
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