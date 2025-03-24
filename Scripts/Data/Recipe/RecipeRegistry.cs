using System.Collections.Generic;
using System.Linq;
using Game.Common;
using Game.Common.Abstract;
using Game.Common.Utilities;
using Godot;

namespace Game.Data;

[GlobalClass]
public partial class RecipeRegistry : Registry<Recipe, RecipeRegistry>
{
    protected override string ResourcePath => Constants.RECIPES_PATH;

    public static List<Recipe> GetRecipes(Recipe.Type type) => Resources.Values
        .Where(resource => resource.RecipeType == type)
        .ToList();

    protected override void LoadResources()
    {
        var files = DirAccessUtils.GetFilesRecursively(Instance.Value.ResourcePath);

        foreach (var file in files)
        {
            if (!file.EndsWith(".tres") && !file.EndsWith(".tres.remap")) continue;

            var resource = ResourceLoader.Load<Recipe>(file);

            if (resource == null) continue;

            var id = resource.Result.Item.Id;

            Resources[id] = resource;
        }
    }
}