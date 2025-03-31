using Game.Common;
using Game.Common.Abstract;
using Game.Common.Utilities;
using Game.Entities;
using Godot;

namespace Game.Data;

public partial class EntityRegistry : Registry<PackedScene, EntityRegistry>
{
    protected override string ResourcePath => Constants.ENTITIES_PATH;
    public static Entity GetAsEntity(string id) => Get(id)?.InstantiateOrNull<Entity>();

    protected override void LoadResources()
    {
        var files = DirAccessUtils.GetFilesRecursively(Instance.Value.ResourcePath);

        foreach (var file in files)
        {
            if (!file.EndsWith(".tscn")) continue;

            var scene = ResourceLoader.Load<PackedScene>(file);

            if (scene == null) continue;

            if (scene.Instantiate() is not Entity instance) continue;

            var id = instance.Id;

            if (id == string.Empty) continue;

            instance.QueueFree();

            Resources[id] = scene;
        }
    }
}