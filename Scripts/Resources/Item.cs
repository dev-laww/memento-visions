using System;
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
public partial class Item : Resource, ICloneable
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
        set => SetValue(value);
    }

    [Export(PropertyHint.MultilineText)] public string Description;

    [Export]
    public Texture2D Icon
    {
        get => Engine.IsEditorHint() ? icon : icon ?? sprite;
        private set => icon = value;
    }

    [Export]
    public Texture2D Sprite
    {
        get => Engine.IsEditorHint() ? sprite : sprite ?? icon;
        private set => sprite = value;
    }

    [Export]
    public bool Stackable
    {
        get => IsStackable();
        private set => _stackable = value;
    }

    private Texture2D icon;
    private Texture2D sprite;
    private Type _type = Type.Material;
    private bool _stackable = true;
    private int _value = 1;

    protected virtual bool IsStackable() => _stackable;
    protected virtual Type GetItemType() => _type;
    protected virtual void SetItemType(Type type) => _type = type;

    protected virtual void SetValue(int value)
    {
        if (value <= 0)
        {
            GD.PrintErr("Value cannot be less than 1.");
            return;
        }

        _value = value;
    }

    public override string ToString() => $"{UniqueName} (x{Value})";

    // Additional methods for stacking items
    private bool ValidateStack(Item other = null)
    {
        if (other == null || AreItemsCompatible(other)) return true;

        GD.PrintErr("Items are not compatible for stacking.");
        return false;
    }

    private bool AreItemsCompatible(Item other) =>
        GetType() == other.GetType() &&
        UniqueName == other.UniqueName;

    private Item ModifyValue(int modification)
    {
        if (!ValidateStack()) return this;
        Value += modification;
        return this;
    }

    private Item ScaleValue(int factor, bool isDivision = false)
    {
        if (!ValidateStack()) return this;

        if (isDivision && factor == 0)
        {
            GD.PrintErr("Cannot divide by zero.");
            return this;
        }

        Value = isDivision ? Value / factor : Value * factor;
        return this;
    }

    public static Item operator +(Item item1, Item item2) =>
        !item1.ValidateStack(item2) ? item1 : item1.ModifyValue(item2.Value);

    public static Item operator -(Item item1, Item item2) =>
        !item1.ValidateStack(item2) ? item1 : item1.ModifyValue(-item2.Value);

    public static Item operator *(Item item1, Item item2) =>
        !item1.ValidateStack(item2) ? item1 : item1.ScaleValue(item2.Value);

    public static Item operator /(Item item1, Item item2) =>
        !item1.ValidateStack(item2) ? item1 : item1.ScaleValue(item2.Value, isDivision: true);

    public static Item operator +(Item item, int value) =>
        item.ModifyValue(value);

    public static Item operator -(Item item, int value) =>
        item.ModifyValue(-value);

    public static Item operator *(Item item, int value) =>
        item.ScaleValue(value);

    public static Item operator /(Item item, int value) =>
        item.ScaleValue(value, isDivision: true);

    public static Item operator ++(Item item) =>
        item.ModifyValue(1);

    public static Item operator --(Item item) =>
        item.ModifyValue(-1);

    // Reverse operations with simplified implementation
    public static Item operator +(int value, Item item) => item + value;
    public static Item operator -(int value, Item item) => item - value;
    public static Item operator *(int value, Item item) => item * value;
    public static Item operator /(int value, Item item) => item / value;

    public object Clone() => MemberwiseClone();

    public Item Duplicate() => (Item)Clone();
}