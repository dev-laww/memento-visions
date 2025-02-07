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
public partial class RecipeRegistry() : Registry<Recipe, RecipeRegistry>(Constants.RECIPES_PATH)
{
    protected override Recipe GetResource(string id)
    {
        var targetIdPart = id.Split(":").Last();

        var exactMatch = GetFiles(OS.IsDebugBuild())
            .FirstOrDefault(file => file.Split("/").Last().Contains(targetIdPart));

        if (exactMatch != null && ResourceLoaderUtils.Load<Recipe>(exactMatch, out var res))
            return res;

        return (
            from file in GetFiles()
            let filename = file.Split("/").Last()
            let similarity = Fuzz.PartialRatio(filename, targetIdPart)
            where similarity >= 80
            orderby similarity descending
            select ResourceLoader.Load<Recipe>(file)
            into resource
            where resource.Get("Item").As<Resource>().Get("Id").ToString() == id
            select resource
        ).FirstOrDefault();
    }

    public static List<Recipe> GetRecipes(Recipe.Type type) => [.. GetFileCache().SelectMany(pair => pair.Value)
        .Select(file => ResourceLoader.Load<Recipe>(file))
        .Where(recipe => recipe.RecipeType == type)];
}