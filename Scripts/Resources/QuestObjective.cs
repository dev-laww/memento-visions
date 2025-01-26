using System;
using System.Linq;
using Game.Globals;
using Godot;
using Godot.Collections;

namespace Game.Resources;

[Tool]
[GlobalClass, Icon("res://assets/icons/quest-step.svg")]
public partial class QuestObjective : Resource
{
    public enum ObjectiveType
    {
        Collect,
        Navigate,
        KillEnemy,
        Deliver
    }

    [Export]
    public ObjectiveType Type
    {
        get => type;
        set
        {
            type = value;
            NotifyPropertyListChanged();
        }
    }

    [Export(PropertyHint.MultilineText)] public string Description { get; set; }

    public bool Completed;


    public QuestObjective()
    {
        if (Engine.IsEditorHint()) return;

        InventoryManager.Pickup += OnItemPickup;
    }

    ~QuestObjective()
    {
        if (Engine.IsEditorHint()) return;

        InventoryManager.Pickup -= OnItemPickup;
    }

    // TODO: add properties for enemy

    private ObjectiveType type;
    public ItemGroup[] Items = [];

    public override Array<Dictionary> _GetPropertyList()
    {
        var properties = new Array<Dictionary>();

        switch (Type)
        {
            case ObjectiveType.Collect:
            case ObjectiveType.Deliver:
                properties.Add(new Dictionary
                {
                    { "name", nameof(Items) },
                    { "type", (int)Variant.Type.Array },
                    { "usage", (int)PropertyUsageFlags.Default },
                    { "hint", (int)PropertyHint.ArrayType },
                    { "hint_string", $"24/17:ItemGroup" }
                });
                break;
            case ObjectiveType.Navigate:
            case ObjectiveType.KillEnemy:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return properties;
    }

    private void OnItemPickup(ItemGroup item) => ProcessItem(item, ObjectiveType.Collect);

    private void ProcessItem(ItemGroup item, ObjectiveType type)
    {
        if (Completed) return;

        if (Type != type) return;

        var items = Items.ToList();

        var group = items.Find(i => i.Item.Id == item.Item.Id);

        if (group is null) return;

        group.Quantity -= item.Quantity;

        if (group.Quantity <= 0)
            items.Remove(group);

        if (items.Count == 0) Completed = true;

        Items = [.. items];
    }
}