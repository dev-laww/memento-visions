using System;
using Game.Data;
using Game.Entities;
using Godot;

namespace Game.Autoload;


public partial class GameEvents : Autoload<GameEvents>
{
    public static event Action<Entity.SpawnInfo> EntitySpawned;
    public static event Action<Entity.DeathInfo> EntityDied;

    [Signal] public delegate void SpawnedEventHandler(Entity.SpawnInfo info);
    [Signal] public delegate void DiedEventHandler(Entity.DeathInfo info);
    [Signal] public delegate void QuickUseSlotUpdatedEventHandler(ItemGroup group);

    public override void _Ready()
    {
        Spawned += info => EntitySpawned?.Invoke(info);
        Died += info => EntityDied?.Invoke(info);
    }

    public static void EmitEntitySpawned(Entity.SpawnInfo info) => Instance.EmitSignalSpawned(info);
    public static void EmitEntityDied(Entity.DeathInfo info) => Instance.EmitSignalDied(info);
    public static void EmitQuickUseSlotUpdated(ItemGroup group) => Instance.EmitSignalQuickUseSlotUpdated(group);
}