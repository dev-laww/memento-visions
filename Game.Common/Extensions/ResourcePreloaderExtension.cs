using Godot;

namespace Game.Common.Extensions;

public static class ResourcePreloaderExtension
{
    public static T GetResource<T>(
        this ResourcePreloader loader,
        string name
    ) where T : Resource => (T)loader.GetResource(name);
}