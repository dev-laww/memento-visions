using System.Collections.Generic;
using FuzzySharp;
using Game.Common.Utilities;
using Godot;
using Game.Common;
using Game.World;

namespace Game.Data;

#nullable enable
public partial class LevelRegistry : RefCounted
{
    private static readonly Dictionary<string, string> scenes = [];

    static LevelRegistry()
    {
        LoadScenes();
    }

    public static string? Get(string id)
    {
        scenes.TryGetValue(id, out var scene);

        if (scene != null) return scene;

        var matches = Process.ExtractOne(id, [.. scenes.Keys]);

        if (matches.Score < 80) return null;

        scenes.TryGetValue(matches.Value, out scene);

        return scene;
    }

    public static bool Get(string id, out string? scene)
    {
        scene = Get(id);

        return scene != null;
    }

    private static void LoadScenes()
    {
        var files = DirAccessUtils.GetFilesRecursively(Constants.LEVELS_PATH);

        foreach (var file in files)
        {
            if (!file.EndsWith(".tscn")) continue;

            var level = ResourceLoader.Load<PackedScene>(file).InstantiateOrNull<BaseLevel>();

            if (level == null) continue;

            scenes[level.Id] = file;
        }
    }
}