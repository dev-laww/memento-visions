using FuzzySharp;
using Game.Common.Utilities;
using Godot;

namespace Game.Common.Abstract;

public abstract class Registry<T, TRegistry> : GodotObject
    where T : GodotObject
    where TRegistry : Registry<T, TRegistry>, new()
{
    protected static readonly Lazy<TRegistry> _instance = new(() => new TRegistry());
    protected readonly Dictionary<string, List<string>> _fileCache = [];
    private readonly string _resourcePath;

    protected Registry(string resourcePath)
    {
        _resourcePath = resourcePath;
        if (!_fileCache.ContainsKey(_resourcePath))
        {
            _fileCache[_resourcePath] = DirAccessUtils.GetFilesRecursively(_resourcePath);
        }
    }

    private List<string> GetFiles(bool forceReload = false)
    {
        if (!forceReload && _fileCache.TryGetValue(_resourcePath, out var value)) return value;

        value = DirAccessUtils.GetFilesRecursively(_resourcePath);
        _fileCache[_resourcePath] = value;

        return value;
    }

    protected virtual T? GetResource(string id)
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

    public static T? Get(string id) => _instance.Value.GetResource(id);

    public void InvalidateCache() => _fileCache.Remove(_resourcePath);
    
    public static Dictionary<string, List<string>> GetFileCache() => _instance.Value._fileCache;
}