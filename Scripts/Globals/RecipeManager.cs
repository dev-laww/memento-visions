using System.Collections.Generic;
using System.Linq;
using Game.Utils;
using Game.Utils.Extensions;
using Game.Utils.JSON.Models;
using Godot;
using Item = Game.Resources.Item;
using JsonLoader = Game.Utils.JSON.JSON;
using ItemModel = Game.Utils.JSON.Models.Item;

namespace Game.Globals;

public partial class RecipeManager : Global<RecipeManager>
{
    public static readonly List<Item> CraftableItems = new();
    public static readonly List<Item> PotionAndFoodItems = new();
    private readonly List<Recipe> recipes = new();
    private readonly List<ItemModel> items = new();

    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;

        var craftingRecipes = JsonLoader.Load<List<Recipe>>(Constants.CRAFTING_RECIPES_DATA_PATH);
        var potionAndFoodRecipes = JsonLoader.Load<List<Recipe>>(Constants.POTION_AND_FOOD_RECIPES_DATA_PATH);
        var itemsData = JsonLoader.Load<List<ItemModel>>(Constants.ITEMS_DATA_PATH);
        var weaponsData = JsonLoader.Load<List<ItemModel>>(Constants.WEAPONS_DATA_PATH);

        items.AddRange(itemsData);
        items.AddRange(weaponsData);

        var craftableItems = items.Where(
            data => craftingRecipes.Any(recipe => recipe.Result.UniqueName == data.UniqueName)
        );

        var resources = craftableItems.Select(item => GD.Load<Item>(item.Resource));

        var potionAndFoodItems = items.Where(
            data => potionAndFoodRecipes.Any(recipe => recipe.Result.UniqueName == data.UniqueName)
        );

        var potionAndFoodResources = potionAndFoodItems.Select(item => GD.Load<Item>(item.Resource));

        recipes.AddRange(craftingRecipes);
        recipes.AddRange(potionAndFoodRecipes);
        CraftableItems.AddRange(resources);
        PotionAndFoodItems.AddRange(potionAndFoodResources);
    }

    public static Item CreateItem(string name)
    {
        var items = Instance.items;
        var recipes = Instance.recipes;
        var player = Instance.GetPlayer();

        var item = items.Find(i => i.UniqueName == name)?.ToItem();

        if (item is null)
            return null;

        var recipe = recipes.Find(r => r.Result.UniqueName == item.UniqueName);

        if (recipe is null)
            return null;

        // find ingredients in items and load it to item
        var ingredients = recipe.Ingredients.Select(ingredient =>
        {
            var data = items.Find(i => i.UniqueName == ingredient.UniqueName);
            var resource = GD.Load<Item>(data.Resource);
            resource.Value = ingredient.Amount;

            return resource;
        });

        // check if player has enough ingredients
        var playerInventory = player?.Inventory;

        if (playerInventory is null)
            return null;

        var hasIngredients = ingredients.All(ingredient =>
        {
            var existing = playerInventory.Items.Find(i => i.UniqueName == ingredient.UniqueName);

            if (existing is null)
                return false;

            if (existing.Stackable)
                return existing.Value >= ingredient.Value * item.Value;

            return true;
        });

        return hasIngredients ? item : null;
    }
}