using System;
using System.Collections.Generic;
using Game.Common.Extensions;
using Game.Common.Interfaces;
using Game.Globals;
using Game.Resources;
using Game.UI.Common;
using Game.Utils.Extensions;
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

    protected bool ShouldInteract
    {
        get => shouldInteract;
        set
        {
            shouldInteract = value;
            NotifyPropertyListChanged();
            InitializeUI();
        }
    }

    private string InteractionLabel
    {
        get => interactionLabel;
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

        if (ShouldInteract && InteractionUI != null)
            InteractionUI.Text = InteractionLabel;

        if (Engine.IsEditorHint()) return;

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;

        InteractionUI?.Hide();
    }

    protected virtual void OnBodyEntered(Node2D body)
    {
        if (!ShouldInteract)
        {
            TriggerQuest();
            return;
        }

        InteractionManager.Register(this);
    }

    private void OnBodyExited(Node2D body)
    {
        var player = this.GetPlayer();

        if (player is null) return;

        var isActive = player.QuestManager.IsActive(Quest);

        if (!ShouldInteract || (!isActive && Mode == TriggerMode.Complete)) return;

        InteractionManager.Unregister(this);
    }

    public Vector2 InteractionPosition => GlobalPosition;

    public virtual void Interact() => TriggerQuest();

    private void TriggerQuest()
    {
        var player = this.GetPlayer();

        if (player is null) return;

        var isActive = player.QuestManager.IsActive(Quest);

        switch (Mode)
        {
            case TriggerMode.Start:
                player.QuestManager.Add(Quest);
                break;
            case TriggerMode.Complete:
                if (!isActive) return;
                Objective?.Complete();
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
                { "name", PropertyName.Quest },
                { "type", (int)Variant.Type.Object },
                { "usage", (int)PropertyUsageFlags.Default },
                { "hint_string", "Quest" }
            },
            new()
            {
                { "name", PropertyName.Mode },
                { "type", (int)Variant.Type.Int },
                { "usage", (int)PropertyUsageFlags.Default },
                { "hint", (int)PropertyHint.Enum },
                { "hint_string", "Start,Complete" }
            },
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

        if (Mode == TriggerMode.Start) return properties;

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
            this.EditorAddChild(new CollisionShape2D { Name = "CollisionShape2D", DebugColor = new Color(0.88f, 0.525f, 0.898f, 0.42f) });

        if (!ShouldInteract) return;

        this.AddInteractionUI();
    }

    private void InitializeUI()
    {
        var node = GetNodeOrNull("Node2D");

        if (!IsNodeReady()) return;

        if (!ShouldInteract)
        {
            node?.QueueFree();
            return;
        }

        if (node != null) return;

        this.AddInteractionUI();

        if (InteractionUI != null && ShouldInteract)
            InteractionUI.Text = interactionLabel;
    }
}