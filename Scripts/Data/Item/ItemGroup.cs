using Godot;

namespace Game.Data;

[Tool]
[GlobalClass, Icon("res://assets/icons/item-group.svg")]
public partial class ItemGroup : Resource
{
    [Export]
    public Item Item
    {
        get => _item;
        set
        {
            _item = value;
            NotifyPropertyListChanged();
        }
    }

    [Export]
    public int Quantity
    {
        get => _quantity;
        set
        {
            _quantity = value;
            NotifyPropertyListChanged();
        }
    }

    private Item _item;
    private int _quantity;

    public void Deconstruct(out Item item, out int quantity)
    {
        item = Item;
        quantity = Quantity;
    }

    public override string ToString() => $"<{Item?.Id} (x{Quantity})>";
}