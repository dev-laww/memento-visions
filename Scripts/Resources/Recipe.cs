using System;
using System.Linq;
using Game.Globals;
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

    private ItemGroup[] GetIngredients(int quantity = 1) =>
        quantity switch
        {
            < 1 => throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than 0."),
            1 => Ingredients,
            _ => Ingredients.Select(ingredient =>
                new ItemGroup { Item = ingredient.Item, Quantity = ingredient.Quantity * quantity }
            ).ToArray()
        };

    public bool CanCreate(int quantity = 1)
    {
        var ingredients = GetIngredients(quantity);
        
        return ingredients.All(ingredient =>
            PlayerInventoryManager.HasItem(ingredient.Item, ingredient.Quantity)
        );
    }
    
    public void Create(int quantity = 1)
    {
        if (!CanCreate(quantity)) return;

        var ingredients = GetIngredients(quantity);

        foreach (var ingredient in ingredients)
            PlayerInventoryManager.RemoveItem(ingredient.Item, ingredient.Quantity);

        PlayerInventoryManager.AddItem(Result, quantity);
    }
    
}