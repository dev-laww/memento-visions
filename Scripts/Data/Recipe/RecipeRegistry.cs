using System.Collections.Generic;
using System.Linq;
using Game.Common;
using Game.Common.Abstract;
using Godot;

namespace Game.Data;

[GlobalClass]
public partial class RecipeRegistry : Registry<Recipe, RecipeRegistry>
{
    protected override string ResourcePath => Constants.RECIPES_PATH;

    public static List<Recipe> GetRecipes(Recipe.Type type) => Resources.Values
        .Where(resource => resource.RecipeType == type)
        .ToList();
}