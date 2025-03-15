using System.Collections.Generic;
using FuzzySharp;
using Game.Common.Utilities;
using Godot;

namespace Game.Data;

#nullable enable
public partial class SceneRegistry : RefCounted
{
    private const string WORLD_PATH = "res://Scenes/World/";

    private static readonly Dictionary<string, string> scenes = [];

    static SceneRegistry()
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
        var files = DirAccessUtils.GetFilesRecursively(WORLD_PATH);

        foreach (var file in files)
        {
            if (file.EndsWith(".tres") && file.EndsWith(".tres.remap")) continue;

            var id = file.GetFile().GetBaseName();

            scenes[id] = file;
        }
    }
}

