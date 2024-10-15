using System;
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
    public Texture2D sprite { get; set; }

    [Export]
    public bool Stackable { get; set; } = true;

    public Texture2D Icon
    {
        get => GetTexture(icon, sprite);
    }

    public Texture2D Sprite
    {
        get => GetTexture(sprite, icon);
    }

    [Export]
    public int MaxStack { get; set; } = 9999999;

    public readonly Guid UID = Guid.NewGuid();

    public Item()
    {
        SetMeta("uid", UID.ToString());
    }

    private Texture2D GetTexture(Texture2D primary, Texture2D secondary)
    {
        if (primary == null && secondary == null)
            throw new Exception("Item must have a sprite or secondary icon");

        return primary ?? secondary;
    }
}