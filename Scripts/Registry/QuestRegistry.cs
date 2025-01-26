using System.Collections.Generic;
using System.Linq;
using FuzzySharp;
using Game.Resources;
using Game.Utils;
using Godot;

namespace Game.Registry;

[GlobalClass]
public partial class QuestRegistry : GodotObject
{
    private static readonly List<string> quests = DirAccessUtils.GetFilesRecursively(Constants.QUESTS_PATH);

    public static Quest Get(string id) => (
        from quest in Engine.IsEditorHint() ? DirAccessUtils.GetFilesRecursively(Constants.QUESTS_PATH) : quests
        where Fuzz.PartialRatio(quest.Split("/").Last(), id.Split(":").Last()) >= 80
        select ResourceLoader.Load<Quest>(quest)
        into resource
        where resource.Id == id
        select resource
    ).FirstOrDefault();
}