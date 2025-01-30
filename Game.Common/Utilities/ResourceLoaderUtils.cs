using Godot;

namespace Game.Common.Utilities;

public static class ResourceLoaderUtils
{
    public static bool Load(string path, out GodotObject resource)
    {
        resource = ResourceLoader.Load(path);
        return resource != null;
    }

    public static bool Load<T>(string path, out T resource) where T : GodotObject
    {
        resource = ResourceLoader.Load<T>(path);
        return resource != null;
    }
}