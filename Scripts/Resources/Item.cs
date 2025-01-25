using System.Linq;
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

    public Texture2D Icon;
    public string Name;
    public string UniqueName;
    public Type WeaponType;
    public PackedScene Component;
    public string Description;

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

    public override string ToString() => $"<Item ({UniqueName})>";

    public override Array<Dictionary> _GetPropertyList()
    {
        var properties = new Array<Dictionary>();

        // Add Icon property
        properties.Add(new Dictionary
        {
            { "name", nameof(Icon) },
            { "type", (int)Variant.Type.Object },
            { "usage", (int)PropertyUsageFlags.Default },
            { "hint", (int)PropertyHint.ResourceType },
            { "hint_string", "Texture2D" }
        });

        // Add Name property
        properties.Add(new Dictionary
        {
            { "name", nameof(Name) },
            { "type", (int)Variant.Type.String },
            { "usage", (int)PropertyUsageFlags.Default }
        });

        // Add UniqueName property
        properties.Add(new Dictionary
        {
            { "name", nameof(UniqueName) },
            { "type", (int)Variant.Type.String },
            { "usage", (int)PropertyUsageFlags.Default }
        });

        // Add ItemCategory property
        properties.Add(new Dictionary
        {
            { "name", nameof(ItemCategory) },
            { "type", (int)Variant.Type.Int },
            { "usage", (int)PropertyUsageFlags.Default },
            { "hint", (int)PropertyHint.Enum },
            { "hint_string", "Consumable,Material,Quest,Weapon" }
        });

        // Add WeaponType and Component properties if ItemCategory is Weapon
        if (ItemCategory == Category.Weapon)
        {
            properties.Add(new Dictionary
            {
                { "name", nameof(WeaponType) },
                { "type", (int)Variant.Type.Int },
                { "usage", (int)PropertyUsageFlags.Default },
                { "hint", (int)PropertyHint.Enum },
                { "hint_string", "Dagger,Sword,Gun,Whip" }
            });

            properties.Add(new Dictionary
            {
                { "name", nameof(Component) },
                { "type", (int)Variant.Type.Object },
                { "usage", (int)PropertyUsageFlags.Default },
                { "hint", (int)PropertyHint.ResourceType },
                { "hint_string", "PackedScene" }
            });
        }

        // Add Description property
        properties.Add(new Dictionary
        {
            { "name", nameof(Description) },
            { "type", (int)Variant.Type.String },
            { "usage", (int)PropertyUsageFlags.Default },
            { "hint", (int)PropertyHint.MultilineText }
        });

        return properties;
    }
}