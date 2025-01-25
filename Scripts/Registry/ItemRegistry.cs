using System.Collections.Generic;
using System.Linq;
using FuzzySharp;
using Game.Resources;
using Game.Utils;
using Godot;

namespace Game.Registry;

[GlobalClass]
public partial class ItemRegistry : GodotObject
{
    private static readonly List<string> items = DirAccessUtils.GetFilesFromDirectories(
        Constants.ITEMS_PATH,
        Constants.WEAPONS_PATH
    );

    public static Item Get(string uniqueName) => (
        from item in items.Where(item => Fuzz.PartialRatio(
            item.Split("/").Last(),
            uniqueName.Split(":").Last()) >= 80
        )
        select ResourceLoader.Load<Item>(item)
        into resource
        where resource.UniqueName == uniqueName
        select resource
    ).FirstOrDefault();
}