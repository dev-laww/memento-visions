using System.Linq;
using Godot;

namespace Game.Resources;

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

    [Export] public Texture2D Icon;
    [Export] public string Name;
    [Export] public string UniqueName;
    [Export] public Category ItemCategory = Category.Material;
    [Export(PropertyHint.MultilineText)] public string Description;

    public override string ToString() => $"<{GetType().ToString().Split(".").Last()} ({UniqueName})>";
}