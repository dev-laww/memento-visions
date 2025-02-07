using System.Collections.Generic;
using System.Linq;
using Game.Common;
using Game.Globals;
using Game.Resources;
using Game.Utils.Extensions;
using Godot;

namespace Game.Components.Managers;

[GlobalClass]
public partial class QuestManager : Node
{
    [Signal] public delegate void AddedEventHandler(Quest quest);
    [Signal] public delegate void UpdatedEventHandler(Quest quest);
    [Signal] public delegate void CompletedEventHandler(Quest quest);
    [Signal] public delegate void RemovedEventHandler(Quest quest);

    private readonly List<Quest> quests = [];

    public IReadOnlyList<Quest> Quests => quests;

    public override void _Ready()
    {
        // TODO: load quests from save file
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint()) return;

        var completedQuests = new List<Quest>();

        foreach (var quest in quests)
        {
            quest.Update();

            if (quest.Completed)
            {
                EmitSignalCompleted(quest);
                completedQuests.Add(quest);
                Log.Debug($"{quest} completed.");
            }
            else
            {
                EmitSignalUpdated(quest);
            }
        }

        foreach (var quest in completedQuests)
        {
            Remove(quest.Id);
        }
    }

    public bool IsActive(Quest quest) => quests.Contains(quest);

    public void Add(Quest quest)
    {
        var player = this.GetPlayer();

        if (player is null) return;

        player.InventoryManager.Pickup += quest.OnItemPickup;
        player.InventoryManager.Remove += quest.OnItemRemoved;
        EnemyManager.EnemyDied += quest.OnEnemyDied;

        quests.Add(quest);
        EmitSignalAdded(quest);

        Log.Info($"{quest} added.");
    }

    public void Remove(string id)
    {
        var quest = quests.FirstOrDefault(q => q.Id == id);
        var player = this.GetPlayer();

        if (player is null || quest is null) return;


        player.InventoryManager.Pickup -= quest.OnItemPickup;
        player.InventoryManager.Remove -= quest.OnItemRemoved;
        EnemyManager.EnemyDied -= quest.OnEnemyDied;

        quests.Remove(quest);
        EmitSignalRemoved(quest);

        Log.Info($"{quest} removed.");
    }
}