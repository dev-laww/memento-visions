using Godot;

namespace Game.Resources;

[GlobalClass, Icon("res://assets/icons/item-group.svg")]
public partial class ItemGroup : Resource
{
    [Export] public Item Item;
    [Export] public int Quantity = 1;
    
    public void Deconstruct(out Item item, out int quantity)
    {
        item = Item;
        quantity = Quantity;
    }

    public override string ToString() => $"{Item.UniqueName} (x{Quantity})";
}