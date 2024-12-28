using System.Linq;
using Godot;

namespace Game.Resources;

[Tool]
[GlobalClass]
public partial class Item : Resource
{
    public enum StackSizes
    {
        Default = 64,
        Unstackable = 1,
        Small = 16,
        Medium = 32
    }

    [Export]
    public string Name;

    [Export]
    public string UniqueName;

    [Export(PropertyHint.MultilineText)]
    public string Description;

    [Export]
    private Texture2D icon;

    [Export]
    public Texture2D sprite;

    [Export]
    public bool Stackable
    {
        get => IsStackable();
        set => SetStackable(value);
    }

    [Export(PropertyHint.Enum, "Unstackable (1),Small (16),Medium (32),Default (64)")]
    protected int stackSize
    {
        get => StackSize switch
        {
            (int)StackSizes.Unstackable => 0,
            (int)StackSizes.Small => 1,
            (int)StackSizes.Medium => 2,
            _ => 3
        };
        set
        {
            if (!Stackable)
            {
                StackSize = (int)StackSizes.Unstackable;
                return;
            }

            StackSize = value switch
            {
                0 => (int)StackSizes.Unstackable,
                1 => (int)StackSizes.Small,
                2 => (int)StackSizes.Medium,
                _ => (int)StackSizes.Default
            };
        }
    }

    public int Value
    {
        get => _value;
        set
        {
            if (value < 0)
            {
                GD.PrintErr("Value cannot be less than 0.");
                return;
            }

            _value = value;
        }
    }

    public Texture2D Icon => GetTexture(icon, sprite);
    public Texture2D Sprite => GetTexture(sprite, icon);

    private bool _stackable = true;
    private int _value = 1;
    public int StackSize { get; private set; } = (int)StackSizes.Default;
    protected virtual bool IsStackable() => _stackable;

    protected virtual void SetStackable(bool value)
    {
        _stackable = value;
        stackSize = value ? (int)StackSizes.Default : (int)StackSizes.Unstackable;
        NotifyPropertyListChanged();
    }

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
}