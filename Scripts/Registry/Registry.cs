using System;
using System.Collections.Generic;
using System.Linq;
using FuzzySharp;
using Game.Utils;
using Godot;

namespace Game.Registry;

public abstract partial class Registry<T, TRegistry> : GodotObject
    where T : Resource
    where TRegistry : Registry<T, TRegistry>, new()
{
    private static readonly Lazy<TRegistry> _instance = new(() => new TRegistry());
    private static readonly Dictionary<string, List<string>> _fileCache = new();
    private readonly string _resourcePath;

    protected Registry(string resourcePath)
    {
        _resourcePath = resourcePath;
        if (!_fileCache.ContainsKey(_resourcePath))
        {
            _fileCache[_resourcePath] = DirAccessUtils.GetFilesRecursively(_resourcePath);
        }
    }

    protected List<string> GetFiles(bool forceReload = false)
    {
        if (!forceReload && _fileCache.TryGetValue(_resourcePath, out var value)) return value;

        value = DirAccessUtils.GetFilesRecursively(_resourcePath);
        _fileCache[_resourcePath] = value;

        return value;
    }

    public T GetResource(string id)
    {
        var targetIdPart = id.Split(":").Last();

        var exactMatch = GetFiles(Engine.IsEditorHint())
            .FirstOrDefault(file => file.Split("/").Last().Equals(targetIdPart));

        if (exactMatch != null && ResourceLoaderUtils.Load<T>(exactMatch, out var res))
            return res;

        return (
            from file in GetFiles()
            let filename = file.Split("/").Last()
            let similarity = Fuzz.PartialRatio(filename, targetIdPart)
            where similarity >= 80
            orderby similarity descending
            select ResourceLoader.Load<T>(file)
            into resource
            where resource.Get("Id").ToString() == id
            select resource
        ).FirstOrDefault();
    }

    public static T Get(string id) => _instance.Value.GetResource(id);

    public void InvalidateCache() => _fileCache.Remove(_resourcePath);
}