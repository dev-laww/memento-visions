using System.Collections.Generic;
using System.Linq;
using FuzzySharp;
using Game.Resources;
using Game.Utils;
using Godot;

namespace Game.Registry;

public static class ItemRegistry
{
    private static readonly List<string> items = DirAccessUtils.GetFilesRecursively(
        Constants.ITEMS_PATH,
        Constants.WEAPONS_PATH
    );

    public static Item Get(string uniqueName) => Get<Item>(uniqueName);

    public static T Get<T>(string uniqueName) where T : Item => (
        from item in items.Where(item => Fuzz.PartialRatio(
            item.Split("/").Last(),
            uniqueName.Split(":").Last()) >= 80
        )
        select ResourceLoader.Load<Item>(item)
        into resource
        where resource.UniqueName == uniqueName
        select resource as T
    ).FirstOrDefault();
}