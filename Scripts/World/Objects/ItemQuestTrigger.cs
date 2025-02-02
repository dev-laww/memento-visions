using System.Collections.Generic;
using Game.Components.Area;
using Game.Globals;
using Game.Resources;
using Game.Utils.Extensions;
using Godot;
using Godot.Collections;
using GodotUtilities;

namespace Game.World.Objects;

[Tool]
[Scene, Icon("res://assets/icons/item-component.svg")]
public partial class ItemQuestTrigger : QuestTrigger
{
    [Node] private Sprite2D Sprite;

    private ItemGroup ItemGroup
    {
        get => itemGroup;
        set
        {
            itemGroup = value;

            if (value is not null) value.PropertyListChanged += Initialize;

            Initialize();
        }
    }

    private ItemGroup itemGroup;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        base._Ready();

        Initialize();
    }

    public override Array<Dictionary> _GetPropertyList()
    {
        var properties = new Array<Dictionary>
        {
            new()
            {
                { "name", PropertyName.ItemGroup },
                { "type", (int)Variant.Type.Object },
                { "hint", (int)PropertyHint.ResourceType },
                { "hint_string", "ItemGroup" },
                { "usage", (int)PropertyUsageFlags.Default }
            }
        };

        properties.AddRange(base._GetPropertyList());

        return properties;
    }

    public override void Interact()
    {
        base.Interact();

        Pick();
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (ItemGroup is null)
            warnings.Add("ItemGroup is not set.");

        if (ItemGroup is not null && ItemGroup.Item is null)
            warnings.Add("Item is not set.");

        if (ItemGroup is not null && ItemGroup.Quantity <= 0)
            warnings.Add("Quantity is less than or equal to 0.");

        warnings.AddRange(base._GetConfigurationWarnings());

        return [.. warnings];
    }

    private void Initialize()
    {
        if (!IsNodeReady() || Sprite is null) return;

        Sprite.Texture = ItemGroup?.Item?.Icon ?? ResourceLoader.Load<Texture2D>("res://assets/items/unknown.png");
        Sprite.NotifyPropertyListChanged();

        UpdateConfigurationWarnings();
        NotifyPropertyListChanged();
    }

    protected override void OnBodyEntered(Node2D body)
    {
        base.OnBodyEntered(body);

        if (ShouldInteract) return;

        Pick();
    }

    private void Pick()
    {
        if (this.GetPlayer() is null || Engine.IsEditorHint() || ItemGroup is null) return;

        InventoryManager.AddItem(ItemGroup);
        QueueFree();
    }
}