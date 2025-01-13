using Godot;

namespace Game.Resources;

[GlobalClass, Icon("res://assets/icons/recipe.svg")]
public partial class Recipe : Resource
{
    [Export] public Item Result;
    [Export] public int Quantity = 1;
    [Export] public Ingredient[] Ingredients = [];
}