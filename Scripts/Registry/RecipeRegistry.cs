using System.Collections.Generic;
using System.Linq;
using FuzzySharp;
using Game.Resources;
using Game.Utils;
using Godot;

namespace Game.Registry;

[GlobalClass]
public partial class RecipeRegistry : GodotObject
{
    private static readonly List<string> recipes = DirAccessUtils.GetFilesRecursively(Constants.RECIPES_PATH);

    public static Recipe Get(string uniqueName) => (
        from recipe in recipes.Where(recipe => Fuzz.PartialRatio(
            recipe.Split("/").Last(),
            uniqueName.Split(":").Last()) >= 80
        )
        select ResourceLoader.Load<Recipe>(recipe)
        into resource
        where resource.Result.Item.UniqueName == uniqueName
        select resource
    ).FirstOrDefault();

    public static List<Recipe> GetRecipes(Recipe.Type? type = null) => recipes.Select(
        recipe => ResourceLoader.Load<Recipe>(recipe)
    ).Where(
        resource => type == null || resource.RecipeType == type
    ).ToList();
}