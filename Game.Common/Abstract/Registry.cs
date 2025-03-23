using Game.Common.Utilities;
using Godot;
using FuzzySharp;

namespace Game.Common.Abstract;

public abstract class Registry<T, TRegistry> : RefCounted
    where T : Resource
    where TRegistry : Registry<T, TRegistry>, new()
{
    protected static readonly Lazy<TRegistry> Instance = new(() => new TRegistry());

    protected abstract string ResourcePath { get; }

    protected static readonly Dictionary<string, T> Resources = [];

    static Registry()
    {
        Instance.Value.LoadResources();
    }

    public static T? Get(string id)
    {
        Resources.TryGetValue(id, out var resource);

        if (resource != null) return (T)resource.Duplicate();

        var matches = Process.ExtractOne(id, [.. Resources.Keys]);

        if (matches == null || matches.Score < 80) return null;

        Resources.TryGetValue(matches.Value, out resource);

        return resource?.Duplicate() as T;
    }

    public static bool Get(string id, out T? resource)
    {
        resource = Get(id);

        return resource != null;
    }

    protected virtual void LoadResources()
    {
        var files = DirAccessUtils.GetFilesRecursively(Instance.Value.ResourcePath);

        foreach (var file in files)
        {
            if (!file.EndsWith(".tres") && !file.EndsWith(".tres.remap")) continue;

            var resource = ResourceLoader.Load<T>(file);

            if (resource == null) continue;

            var id = resource.Get("Id").AsString();

            if (id == string.Empty) continue;

            Resources[id] = resource;
        }
    }
}