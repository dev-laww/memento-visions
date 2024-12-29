using System.Linq;
using Godot;

namespace Game.Resources;

public enum Type
{
    Material,
    Consumable,
    Weapon,
    Quest
}

[Tool]
[GlobalClass]
public partial class Item : Resource
{
    [Export] public string Name;
    [Export] public string UniqueName;

    [Export]
    public Type Type
    {
        get => GetItemType();
        private set => SetItemType(value);
    }

    [Export]
    public int Value
    {
        get => _value;
        private set => SetValue(value);
    }

    [Export(PropertyHint.MultilineText)] public string Description;
    [Export] private Texture2D icon;
    [Export] public Texture2D sprite;

    [Export]
    public bool Stackable
    {
        get => IsStackable();
        private set => _stackable = value;
    }

    public Texture2D Icon => GetTexture(icon, sprite);
    public Texture2D Sprite => GetTexture(sprite, icon);

    private Type _type = Type.Material;
    private bool _stackable = true;
    private int _value = 1;

    private static Texture2D GetTexture(Texture2D primary, Texture2D secondary)
    {
        if (primary != null || secondary != null) return primary ?? secondary;

        return null;
    }

    public static Item operator +(Item item1, Item item2)
    {
        var conditions = new[]
        {
            item1.GetType() == item2.GetType(),
            item1.UniqueName == item2.UniqueName,
            item1.Stackable && item2.Stackable
        };

        if (conditions.Contains(false))
        {
            GD.PrintErr("Items are not stackable.");
            return item1;
        }

        item1.Value += item2.Value;
        return item1;
    }

    public static Item operator -(Item item1, Item item2)
    {
        var conditions = new[]
        {
            item1.GetType() == item2.GetType(),
            item1.UniqueName == item2.UniqueName,
            item1.Stackable && item2.Stackable
        };

        if (conditions.Contains(false))
        {
            GD.PrintErr("Items are not stackable.");
            return item1;
        }

        item1.Value -= item2.Value;
        return item1;
    }

    protected virtual bool IsStackable() => _stackable;
    protected virtual Type GetItemType() => _type;
    protected virtual void SetItemType(Type type) => _type = type;

    protected virtual void SetValue(int value)
    {
        if (value < 0)
        {
            GD.PrintErr("Value cannot be less than 0.");
            return;
        }

        _value = value;
    }
}