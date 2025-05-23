using Game.Common.Extensions;
using Game.Common.Interfaces;
using Game.Autoload;
using Game.UI.Common;
using Godot;
using GodotUtilities;
using System.Linq;

namespace Game.Components;

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
    private string interactionLabel = "Interact";
    private bool isInteractable = true;

    public override void _EnterTree()
    {
        this.AddInteractionUI();

        if (this.GetChildrenOfType<CollisionShape2D>().Any()) return;

        var collisionShape = new CollisionShape2D { Name = "CollisionShape2D", DebugColor = new Color(0.88f, 0.525f, 0.898f, 0.42f) };

        this.EditorAddChild(collisionShape);
    }

    public override void _Ready()
    {
        CollisionLayer = 1 << 4;
        CollisionMask = 1 << 2;
        NotifyPropertyListChanged();

        if (InteractionUI == null) return;

        InteractionUI.Text = interactionLabel;

        if (Engine.IsEditorHint()) return;

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;

        InteractionUI.Hide();
    }

    public Vector2 InteractionPosition => GlobalPosition;

    public void Interact()
    {
        if (!isInteractable) return;

        EmitSignalInteracted();
    }

    public void Toggle(bool value) => isInteractable = value;

    public void HideUI() => InteractionUI.AnimateHide();

    public void ShowUI() => InteractionUI.AnimateShow();

    private void OnBodyEntered(Node _)
    {
        if (!isInteractable) return;

        InteractionManager.Register(this);
    }

    private void OnBodyExited(Node _)
    {
        if (!isInteractable) return;

        InteractionManager.Unregister(this);
    }
}