using System;
using System.Linq;
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

    public ItemRequirement[] Items = [];

    public bool Completed;

    // TODO: add properties for enemy

    private ObjectiveType type;

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
                    { "hint_string", $"24/17:ItemRequirement" }
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

    public void UpdateItemProgress(ItemGroup group)
    {
        if (Type != ObjectiveType.Collect && Type != ObjectiveType.Deliver)
        {
            GD.PushWarning($"{this} does not require items. Use the appropriate method instead.");
            throw new InvalidOperationException();
        }

        if (Completed) return;

        foreach (var requirement in Items)
        {
            if (requirement.Item.Id != group.Item.Id) continue;

            requirement.Quantity = Mathf.Min(requirement.Required, requirement.Quantity + group.Quantity);
        }

        Completed = Items.ToList().All(requirement => requirement.Quantity >= requirement.Required);
    }
    public override string ToString() => $"<QuestObjective ({Type} {GetHashCode()})>";
}