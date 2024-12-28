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
        Medium = 32,
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
    public int StackSize
    {
        get => _stackSize switch
        {
            StackSizes.Unstackable => 0,
            StackSizes.Small => 1,
            StackSizes.Medium => 2,
            _ => 3
        };
        set
        {
            if (!Stackable)
            {
                _stackSize = StackSizes.Unstackable;
                return;
            }

            _stackSize = value switch
            {
                0 => StackSizes.Unstackable,
                1 => StackSizes.Small,
                2 => StackSizes.Medium,
                _ => StackSizes.Default
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
    private StackSizes _stackSize = StackSizes.Default;
    protected virtual bool IsStackable() => _stackable;

    protected virtual void SetStackable(bool value)
    {
        _stackable = value;
        StackSize = value ? (int)StackSizes.Default : (int)StackSizes.Unstackable;
        NotifyPropertyListChanged();
    }

    private Texture2D GetTexture(Texture2D primary, Texture2D secondary)
    {
        if (primary != null || secondary != null) return primary ?? secondary;

        GD.PrintErr("Both primary and secondary textures are null.");
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
}