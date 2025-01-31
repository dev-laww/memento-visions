using System;
using System.Collections.Generic;
using Game.Common.Extensions;
using Game.Common.Interfaces;
using Game.Globals;
using Game.Resources;
using Game.UI.Common;
using Godot;
using Godot.Collections;

namespace Game.Components.Area;

[Tool]
[GlobalClass]
public partial class QuestTrigger : Area2D, IInteractable
{
    private enum TriggerMode
    {
        Start,
        Complete
    }

    [Export]
    private TriggerMode Mode
    {
        get => mode;
        set
        {
            mode = value;
            NotifyPropertyListChanged();
        }
    }

    public Quest Quest
    {
        get => _quest;
        set
        {
            _quest = value;
            Objective = value?.Objectives[ObjectiveIndex];
            NotifyPropertyListChanged();
            UpdateConfigurationWarnings();
        }
    }

    private QuestObjective Objective;

    private int ObjectiveIndex
    {
        get => _objectiveIndex;
        set
        {
            var index = Math.Max(0, Math.Min(value, Quest?.Objectives.Count - 1 ?? 0));
            _objectiveIndex = index;
            Objective = Quest?.Objectives[index];
            NotifyPropertyListChanged();
        }
    }

    private bool ShouldInteract
    {
        get => shouldInteract;
        set
        {
            shouldInteract = value;
            NotifyPropertyListChanged();

            if (!value)
            {
                GetNodeOrNull("Node2D")?.QueueFree();
                return;
            }

            this.AddInteractionUI();
        }
    }

    private string InteractionLabel
    {
        get => InteractionUI?.Text ?? string.Empty;
        set
        {
            interactionLabel = value;

            if (InteractionUI == null) return;

            InteractionUI.Text = value;
        }
    }

    private bool shouldInteract;
    private string interactionLabel;
    private InteractionUI InteractionUI => GetNodeOrNull<InteractionUI>("Node2D/InteractionUI");
    private Quest _quest;
    private int _objectiveIndex;
    private TriggerMode mode;

    public override void _Ready()
    {
        CollisionLayer = 1 << 4;
        CollisionMask = 1 << 2;
        NotifyPropertyListChanged();

        if (InteractionUI != null && ShouldInteract)
            InteractionUI.Text = interactionLabel;

        if (Engine.IsEditorHint()) return;

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;

        InteractionUI?.Hide();
    }

    private void OnBodyEntered(Node2D body)
    {
        if (!ShouldInteract)
        {
            switch (Mode)
            {
                case TriggerMode.Start:
                    Quest?.Start();
                    break;
                case TriggerMode.Complete:
                    if (!Quest.IsActive) return;
                    Objective?.Complete();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return;
        }

        if (!Quest.IsActive && Mode == TriggerMode.Complete) return;

        InteractionManager.Register(this);
    }

    private void OnBodyExited(Node2D body)
    {
        if (!ShouldInteract || (!Quest.IsActive && Mode == TriggerMode.Complete)) return;
        InteractionManager.Unregister(this);
    }

    public Vector2 InteractionPosition => GlobalPosition;

    public void Interact()
    {
        switch (Mode)
        {
            case TriggerMode.Start:
                if (Quest?.IsActive ?? false) return;
                Quest?.Start();
                break;
            case TriggerMode.Complete:
                if (!Quest?.IsActive ?? false) return;
                Quest?.CompleteObjective(ObjectiveIndex);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void ShowUI() => InteractionUI?.Show();
    public void HideUI() => InteractionUI?.Hide();


    public override void _ValidateProperty(Dictionary property)
    {
        var name = property["name"].ToString();

        if (name != PropertyName.ObjectiveIndex) return;

        property["hint_string"] = $"0,{Quest?.Objectives.Count - 1},1";
    }

    public override Array<Dictionary> _GetPropertyList()
    {
        var properties = new Array<Dictionary>
        {
            new()
            {
                { "name", PropertyName.ShouldInteract },
                { "type", (int)Variant.Type.Bool },
                { "usage", (int)PropertyUsageFlags.Default }
            }
        };

        if (ShouldInteract)
            properties.Add(new Dictionary
            {
                { "name", PropertyName.InteractionLabel },
                { "type", (int)Variant.Type.String },
                { "usage", (int)PropertyUsageFlags.Default }
            });

        properties.Add(new Dictionary
        {
            { "name", PropertyName.Quest },
            { "type", (int)Variant.Type.Object },
            { "usage", (int)PropertyUsageFlags.Default },
            { "hint_string", "Quest" }
        });

        if (Mode != TriggerMode.Complete) return properties;

        properties.Add(new Dictionary
        {
            { "name", PropertyName.Objective },
            { "type", (int)Variant.Type.Object },
            { "usage", (int)(PropertyUsageFlags.Default | PropertyUsageFlags.ReadOnly) },
            { "hint_string", "QuestObjective" }
        });
        properties.Add(new Dictionary
        {
            { "name", PropertyName.ObjectiveIndex },
            { "type", (int)Variant.Type.Int },
            { "usage", (int)PropertyUsageFlags.Default }
        });

        return properties;
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (Quest == null)
            warnings.Add("Quest is not set.");

        return [.. warnings];
    }

    public override void _EnterTree()
    {
        if (GetNodeOrNull("CollisionShape2D") == null)
        {
            var collision = new CollisionShape2D
            {
                Name = "CollisionShape2D",
                DebugColor = new Color(0.88f, 0.525f, 0.898f, 0.42f)
            };

            AddChild(collision);
            collision.SetOwner(GetTree().GetEditedSceneRoot());
        }

        if (!ShouldInteract) return;

        this.AddInteractionUI();
    }
}