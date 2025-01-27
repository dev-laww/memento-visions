using Godot;


namespace Game.Resources;

[Tool]
[GlobalClass]
public partial class ItemRequirement : Resource
{ 
    [Export] public Item Item;
    [Export] public int Amount;
    public int Quantity;
}