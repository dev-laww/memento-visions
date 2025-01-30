using System.Collections.Generic;
using System.Linq;
using FuzzySharp;
using Game.Common;
using Game.Common.Utilities;
using Game.Resources;
using Godot;

namespace Game.Registry;

[GlobalClass]
public partial class RecipeRegistry : GodotObject
{
    private static readonly List<string> recipes = DirAccessUtils.GetFilesRecursively(Constants.RECIPES_PATH);

    public static Recipe Get(string id) => (
        from recipe in Engine.IsEditorHint() ? DirAccessUtils.GetFilesRecursively(Constants.RECIPES_PATH) : recipes
        where Fuzz.PartialRatio(recipe.Split("/").Last(), id.Split(":").Last()) >= 80
        select ResourceLoader.Load<Recipe>(recipe)
        into resource
        where resource.Result.Item.Id == id
        select resource
    ).FirstOrDefault();

    public static List<Recipe> GetRecipes(Recipe.Type? type = null) =>
    [
        .. recipes.Select(
            recipe => ResourceLoader.Load<Recipe>(recipe)
        ).Where(resource => type == null || resource.RecipeType == type)
    ];
}