using System;
using System.Collections.Generic;
using System.Linq;
using FuzzySharp;
using Game.Utils;
using Godot;

namespace Game.Registry;

public abstract partial class Registry<T> : GodotObject where T : Resource
{
    // Cache files per resource path using a static dictionary
    private static readonly Dictionary<string, List<string>> _fileCache = new();
    private readonly string _resourcePath;

    protected Registry(string resourcePath)
    {
        _resourcePath = resourcePath;
        // Initialize cache for this resource path
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

        // First pass: Try exact match
        var exactMatch = GetFiles(Engine.IsEditorHint()).FirstOrDefault(file => file.Split("/").Last().Equals(targetIdPart));

        if (exactMatch != null && ResourceLoaderUtils.Load<T>(exactMatch, out var res))
            return res;

        // Second pass: Fuzzy match if exact not found
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

    // Optional: Add cache invalidation method
    public void InvalidateCache() => _fileCache.Remove(_resourcePath);
}