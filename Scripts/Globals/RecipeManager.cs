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
    public static List<Item> CraftableItems => Instance.craftableItems.Select(item => item.Duplicate()).ToList();

    public static List<Item> PotionAndFoodItems =>
        Instance.potionAndFoodItems.Select(item => item.Duplicate()).ToList();

    private readonly List<Item> craftableItems = new();
    private readonly List<Item> potionAndFoodItems = new();
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

        var crafts = items.Where(
            data => craftingRecipes.Any(recipe => recipe.Result.UniqueName == data.UniqueName)
        );

        var resources = crafts.Select(item => GD.Load<Item>(item.Resource));

        var potionsAndFood = items.Where(
            data => potionAndFoodRecipes.Any(recipe => recipe.Result.UniqueName == data.UniqueName)
        );

        var potionAndFoodResources = potionsAndFood.Select(item => GD.Load<Item>(item.Resource));

        recipes.AddRange(craftingRecipes);
        recipes.AddRange(potionAndFoodRecipes);
        craftableItems.AddRange(resources);
        potionAndFoodItems.AddRange(potionAndFoodResources);
    }

    public static Item CreateItem(Item item)
    {
        var items = Instance.items;
        var recipes = Instance.recipes;
        var player = Instance.GetPlayer();

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
        }).ToList();

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

        if (!hasIngredients) return null;

        // remove ingredients from player inventory
        ingredients.ForEach(ingredient => playerInventory.RemoveItem(ingredient));

        // add item to player inventory
        playerInventory.AddItem(item);

        return item;
    }
}