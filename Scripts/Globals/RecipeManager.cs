using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Game.Resources;
using Game.Utils;
using Godot;

namespace Game.Globals;

public partial class RecipeManager : Global<RecipeManager>
{
    private readonly ReadOnlyDictionary<Recipe.Type, List<Recipe>> Recipes = new(
        new Dictionary<Recipe.Type, List<Recipe>>
        {
            { Recipe.Type.Craftable, [] },
            { Recipe.Type.Consumable, [] }
        }
    );

    public override void _Ready()
    {
        LoadRecipes();
    }

    private void LoadRecipes()
    {
        var dir = DirAccess.Open(Constants.RECIPES_PATH);

        if (dir is null)
        {
            GD.PrintErr("Recipes directory not found.");
            return;
        }

        dir.ListDirBegin();

        var file = dir.GetNext();

        while (file != "")
        {
            if (file.GetExtension() == "tres")
            {
                var recipe = GD.Load<Recipe>(Constants.RECIPES_PATH + file);

                if (recipe is not null) Recipes[recipe.RecipeType].Add(recipe);
            }

            file = dir.GetNext();
        }

        dir.ListDirEnd();
    }

    public static IReadOnlyList<Recipe> GetRecipesFromType(Recipe.Type type) => Instance.Recipes[type].Where(
        recipe => recipe.Unlocked
    ).ToList();

    public static Recipe GetRecipeFromResult(Item item)
    {
        var keys = Instance.Recipes.Keys;

        return keys.Select(key => Instance.Recipes[key].Find(
            r => r.Result.UniqueName == item.UniqueName
        )).FirstOrDefault(recipe => recipe is not null);
    }
}