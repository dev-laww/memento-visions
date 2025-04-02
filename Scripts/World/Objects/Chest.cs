using System.Collections.Generic;
using Game.Components;
using Game.Data;
using Godot;
using GodotUtilities;

namespace Game.World;

[Tool]
[Scene]
public partial class Chest : Node2D
{
    [Export]
    private LootTable Loot
    {
        get => loot;
        set
        {
            loot = value;

            if (loot.IsConnected("property_list_changed", Callable.From(SetDrops))) return;

            loot.Connect("property_list_changed", Callable.From(SetDrops));
        }
    }

    [Node] private DropManager dropManager;
    [Node] private Interaction interaction;

    private LootTable loot;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        interaction.Interacted += OnInteracted;
    }

    private void OnInteracted()
    {
        dropManager.SpawnDrops(GlobalPosition);
        QueueFree();
    }

    private void SetDrops() => dropManager.Drops = loot?.Drops;
}