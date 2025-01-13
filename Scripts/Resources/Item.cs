using Godot;

namespace Game.Resources;

public enum ItemCategory
{
    Consumable,
    Equipment,
    Material,
    Quest,
    Weapon
}

[GlobalClass, Icon("res://assets/icons/item.svg")]
public partial class Item : Resource
{
    [Export] public string Name;
    [Export] public string UniqueName;
    [Export(PropertyHint.MultilineText)] public string Description;
    [Export] public Texture Icon;
    [Export] public ItemCategory Category = ItemCategory.Material;
}