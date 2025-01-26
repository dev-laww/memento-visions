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
    private static List<string> Items => DirAccessUtils.GetFilesRecursively(Constants.ITEMS_PATH);

    public static Item Get(string id) => (
        from item in Engine.IsEditorHint() ? DirAccessUtils.GetFilesRecursively(Constants.ITEMS_PATH) : Items
        where Fuzz.PartialRatio(item.Split("/").Last(), id.Split(":").Last()) >= 80
        select ResourceLoader.Load<Item>(item)
        into resource
        where resource.Id == id
        select resource
    ).FirstOrDefault();
}