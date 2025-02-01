using System.Collections.Generic;
using System.Linq;
using Game.Common;
using Game.Common.Abstract;
using Game.Resources;
using Godot;

namespace Game.Registry;

[GlobalClass]
public partial class RecipeRegistry() : Registry<Recipe, RecipeRegistry>(Constants.RECIPES_PATH)
{
    public static List<Recipe> GetRecipes(Recipe.Type type) => [.. GetFileCache().SelectMany(pair => pair.Value)
        .Select(file => ResourceLoader.Load<Recipe>(file))
        .Where(recipe => recipe.RecipeType == type)];
}