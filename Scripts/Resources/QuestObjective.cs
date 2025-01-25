using System;
using System.Collections.Generic;
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
        Give
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

    private ItemGroup[] items = [];
    public List<ItemGroup> Items => [.. items];


    // TODO: add properties for enemy

    private ObjectiveType type;

    public override Array<Dictionary> _GetPropertyList()
    {
        var properties = new Array<Dictionary>();

        switch (Type)
        {
            case ObjectiveType.Collect:
            case ObjectiveType.Give:
                properties.Add(new Dictionary
                {
                    { "name", nameof(items) },
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
}