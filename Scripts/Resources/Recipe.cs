using System;
using System.Linq;
using Game.Common;
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

    [Export] public ItemGroup Result;
    [Export] public Type RecipeType = Type.Craftable;
    [Export] private ItemGroup[] Ingredients = [];

    public bool Unlocked;

    private ItemGroup[] GetIngredients(int quantity = 1) =>
        quantity switch
        {
            < 1 => throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than 0."),
            1 => Ingredients,
            _ => [.. Ingredients.Select(ingredient =>
                new ItemGroup { Item = ingredient.Item, Quantity = ingredient.Quantity * quantity }
            )]
        };

    public bool CanCreate(int quantity = 1)
    {
        var ingredients = GetIngredients(quantity);
        
        return ingredients.All(InventoryManager.HasItem);
    }
    
    public void Create(int quantity = 1)
    {
        if (!CanCreate(quantity)) return;

        var ingredients = GetIngredients(quantity);

        foreach (var ingredient in ingredients)
            InventoryManager.RemoveItem(ingredient);

        var item = new ItemGroup { Item = Result.Item, Quantity = Result.Quantity * quantity };
        InventoryManager.AddItem(item);
        Log.Debug($"Created {item}.");
    }

    public override string ToString() => $"<Recipe ({Result})>";
}