using System;
using System.Linq;
using Godot;

namespace Game.Resources;

[GlobalClass, Icon("res://assets/icons/recipe.svg")]
public partial class Recipe : Resource
{
    public enum Type
    {
        Craftable,
        Consumable,
    }

    [Export] public Item Result;
    [Export] public int Quantity = 1;
    [Export] public Type RecipeType = Type.Craftable;
    [Export] private ItemGroup[] Ingredients = [];

    public bool Unlocked;

    public ItemGroup[] GetIngredients(int quantity = 1) =>
        quantity switch
        {
            < 1 => throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than 0."),
            1 => Ingredients,
            _ => Ingredients.Select(ingredient =>
                new ItemGroup { Item = ingredient.Item, Quantity = ingredient.Quantity * quantity }
            ).ToArray()
        };
}