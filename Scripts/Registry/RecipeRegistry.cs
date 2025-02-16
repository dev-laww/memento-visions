using System.Collections.Generic;
using System.Linq;
using FuzzySharp;
using Game.Common;
using Game.Common.Abstract;
using Game.Common.Utilities;
using Game.Resources;
using Godot;

namespace Game.Registry;

[GlobalClass]
public partial class RecipeRegistry : Registry<Recipe, RecipeRegistry>
{
    protected override string ResourcePath => Constants.RECIPES_PATH;

    public static List<Recipe> GetRecipes(Recipe.Type type) => Resources.Values
        .Where(resource => resource.RecipeType == type)
        .ToList();
}