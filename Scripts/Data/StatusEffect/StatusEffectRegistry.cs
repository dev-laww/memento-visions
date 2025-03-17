using Game.Common;
using Game.Common.Abstract;
using Game.Common.Utilities;
using Game.Components;
using Game.Entities;
using Godot;

namespace Game.Data;

public partial class StatusEffectRegistry : Registry<PackedScene, StatusEffectRegistry>
{
    protected override string ResourcePath => Constants.STATUS_EFFECTS_PATH;
    public static StatusEffect GetAsStatusEffect(string id) => Get(id)?.InstantiateOrNull<StatusEffect>();

    protected override void LoadResources()
    {
        var files = DirAccessUtils.GetFilesRecursively(Instance.Value.ResourcePath);

        foreach (var file in files)
        {
            if (!file.EndsWith(".tscn")) continue;

            var scene = ResourceLoader.Load<PackedScene>(file);

            if (scene == null) continue;

            var instance = scene.Instantiate<StatusEffect>();
            var id = instance.Id;

            if (id == string.Empty) continue;

            instance.QueueFree();

            Resources[id] = scene;
        }
    }
}