using System.Linq;
using Game.Autoload;
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

            if (loot == null) return;
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
        var spawnPosition = GlobalPosition;
        var drops = dropManager.SpawnDrops(spawnPosition).ToList();

        if (drops.Count != 0)
        {
            for (var i = 0; i < drops.Count; i++)
            {
                var item = drops[i];

                GetTree().CreateTimer(0.3f * i).Timeout += () =>
                {
                    var text = FloatingTextManager.SpawnFloatingText($"x{item.Quantity} {item.Item.Name}", spawnPosition);

                    text.Finished += text.QueueFree;
                };
            }
        }
        else
        {
            var text = FloatingTextManager.SpawnFloatingText("Nothing", spawnPosition, Colors.Gray);

            text.Finished += text.QueueFree;
        }

        QueueFree();
    }

    private void SetDrops() => dropManager.Drops = loot?.Drops;
}