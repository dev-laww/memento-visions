using Godot;


namespace Game.Data;

[Tool]
[GlobalClass]
public partial class ItemRequirement : Resource
{ 
    [Export] public Item Item;
    [Export] public int Amount;
    public int Quantity;
    
    public override string ToString() => $"<ItemRequirement ({Item.Id} x{Amount})>";
}