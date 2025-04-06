using Game.Common;
using Game.Common.Abstract;
using Game.Common.Utilities;
using Game.World;
using Godot;

namespace Game.Data;

public partial class LevelRegistry : Registry<PackedScene, LevelRegistry>
{
    protected override string ResourcePath => Constants.LEVELS_PATH;

    protected override void LoadResources()
    {
        var files = DirAccessUtils.GetFilesRecursively(Instance.Value.ResourcePath);

        foreach (var file in files)
        {
            if (!file.EndsWith(".tscn")) continue;

            var resource = ResourceLoader.Load<PackedScene>(file);

            if (resource == null) continue;

            var level = resource.Instantiate<BaseLevel>();

            Resources[level.Id] = resource;
        }
    }
}