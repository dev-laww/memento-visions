using Godot;

namespace Game.Resources;

[GlobalClass, Icon("res://assets/icons/ingredient.svg")]
public partial class Ingredient : Resource
{
    [Export] public Item Item;
    [Export] public int Quantity = 1;
}