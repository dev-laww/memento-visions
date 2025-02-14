using System;
using Godot;
using Godot.Collections;

namespace Game.Resources;

[Tool]
[GlobalClass, Icon("res://assets/icons/item.svg")]
public partial class Item : Resource
{
    public enum Category
    {
        Consumable,
        Material,
        Quest,
        Weapon
    }

    public enum Type
    {
        Dagger,
        Sword,
        Gun,
        Whip
    }

    public Texture2D Icon { get; private set; }

    public string Name
    {
        get => ResourceName;
        private set
        {
            ResourceName = value;
        }
    }

    public string Id { get; private set; } = Guid.NewGuid().ToString();
    public Type WeaponType { get; private set; }
    public PackedScene Component { get; private set; }
    public string Description { get; private set; }
    public float Damage { get; private set; }

    public Category ItemCategory
    {
        get => category;
        set
        {
            category = value;
            NotifyPropertyListChanged();
        }
    }

    private Category category;

    public override string ToString() => $"<Item ({Id})>";

    public override Array<Dictionary> _GetPropertyList()
    {
        var properties = new Array<Dictionary>
        {
            new()
            {
                { "name", PropertyName.Id },
                { "type", (int)Variant.Type.String },
                { "usage", (int)PropertyUsageFlags.Default }
            },
            new()
            {
                { "name", PropertyName.Icon },
                { "type", (int)Variant.Type.Object },
                { "usage", (int)PropertyUsageFlags.Default },
                { "hint", (int)PropertyHint.ResourceType },
                { "hint_string", "Texture2D" }
            },
            new()
            {
                { "name", PropertyName.Name },
                { "type", (int)Variant.Type.String },
                { "usage", (int)PropertyUsageFlags.Default }
            },
            new()
            {
                { "name", PropertyName.ItemCategory },
                { "type", (int)Variant.Type.Int },
                { "usage", (int)PropertyUsageFlags.Default },
                { "hint", (int)PropertyHint.Enum },
                { "hint_string", "Consumable,Material,Quest,Weapon" }
            }
        };

        if (ItemCategory == Category.Weapon)
        {
            properties.Add(new Dictionary
            {
                { "name", PropertyName.WeaponType },
                { "type", (int)Variant.Type.Int },
                { "usage", (int)PropertyUsageFlags.Default },
                { "hint", (int)PropertyHint.Enum },
                { "hint_string", "Dagger,Sword,Gun,Whip" }
            });

            properties.Add(new Dictionary
            {
                { "name", PropertyName.Component },
                { "type", (int)Variant.Type.Object },
                { "usage", (int)PropertyUsageFlags.Default },
                { "hint", (int)PropertyHint.ResourceType },
                { "hint_string", "PackedScene" }
            });

            properties.Add(new Dictionary
            {
                { "name", PropertyName.Damage },
                { "type", (int)Variant.Type.Float },
                { "usage", (int)PropertyUsageFlags.Default }
            });
        }

        properties.Add(new Dictionary
        {
            { "name", PropertyName.Description },
            { "type", (int)Variant.Type.String },
            { "usage", (int)PropertyUsageFlags.Default },
            { "hint", (int)PropertyHint.MultilineText }
        });

        return properties;
    }
}