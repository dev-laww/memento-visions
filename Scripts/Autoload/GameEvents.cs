using Game.Entities;
using Godot;

namespace Game.Autoload;


public partial class GameEvents : Autoload<GameEvents>
{
    [Signal] public delegate void EntitySpawnedEventHandler(Entity.SpawnInfo info);
    [Signal] public delegate void EntityDiedEventHandler(Entity.DeathInfo info);

    public static void EmitEntitySpawned(Entity.SpawnInfo info) => Instance.EmitSignalEntitySpawned(info);
    public static void EmitEntityDied(Entity.DeathInfo info) => Instance.EmitSignalEntityDied(info);
}