using Godot;

namespace Game.Resources;

public enum ItemType
{
    Consumable,
    Weapon,
    Material,
    Quest
}

[GlobalClass]
[Tool]
public partial class Item : Resource
{
    [Export]
    public string Name { get; set; }

    [Export(PropertyHint.MultilineText)]
    public string Description { get; set; }

    [Export]
    public int Value { get; set; } = 1;

    [Export]
    public ItemType Type { get; set; } = ItemType.Material;

    [Export]
    private Texture2D icon { get; set; }

    [Export]
    public Texture2D Sprite { get; set; }

    [Export]
    public bool Stackable { get; set; } = true;

    public Texture2D Icon => icon ?? Sprite;

    [Export]
    public int MaxStack { get; set; } = 9999999;
}