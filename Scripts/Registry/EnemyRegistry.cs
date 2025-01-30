using Game.Common;
using Game.Common.Abstract;
using Game.Entities.Enemies;
using Godot;

namespace Game.Registry;

[GlobalClass]
public partial class EnemyRegistry() : Registry<PackedScene, EnemyRegistry>(Constants.ENEMIES_PATH)
{
    public new static Enemy Get(string id) => _instance.Value.GetResource(id)?.Instantiate<Enemy>();
}