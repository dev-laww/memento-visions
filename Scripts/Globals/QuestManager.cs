using System.Collections.Generic;
using System.Linq;
using Game.Resources;

namespace Game.Globals;

public partial class QuestManager : Global<QuestManager>
{
    public delegate void AddedEventHandler(Quest quest);
    public delegate void UpdatedEventHandler(Quest quest);
    public delegate void CompletedEventHandler(Quest quest);

    public static event AddedEventHandler Added;
    public static event UpdatedEventHandler Updated;
    public static event CompletedEventHandler Completed;

    private readonly Dictionary<string, Quest> activeQuests = new();

    public static IReadOnlyDictionary<string, Quest> ActiveQuests => Instance.activeQuests;

    public override void _Ready()
    {
        InventoryManager.Pickup += OnItemPickup;
    }

    public static void Add(Quest quest)
    {
        if (!Instance.activeQuests.TryAdd(quest.Id, quest)) return;

        Added?.Invoke(quest);
    }

    public static void Remove(string id)
    {
        if (!Instance.activeQuests.Remove(id, out var quest)) return;

        Updated?.Invoke(quest);
    }

    // TODO: Implement the rest of the QuestManager class

    private void OnItemPickup(ItemGroup item)
    {
        //     var quests = activeQuests.Values;
        //
        //     foreach (var quest in quests)
        //     {
        //         var objectives = quest.Objectives;
        //
        //         if (quest.Ordered)
        //         {
        //             if (objectives.Count == 0 || objectives[0] is not { } objective) continue;
        //
        //             if (objective.Type is not (QuestObjective.ObjectiveType.Collect or QuestObjective.ObjectiveType.Give) ||
        //                 objective.Items.All(itemGroup => itemGroup.Item.UniqueName != item.Item.UniqueName)) continue;
        //
        //             var objItem = objective.Items.First(itemGroup => itemGroup.Item.UniqueName == item.Item.UniqueName);
        //
        //             if (objItem.Quantity > item.Quantity)
        //                 objItem.Quantity -= item.Quantity;
        //             else
        //                 objective.Items.Remove(objItem);
        //
        //             if (objective.Items.Count == 0)
        //             {
        //                 objectives.RemoveAt(0);
        //
        //                 if (objectives.Count == 0)
        //                 {
        //                     activeQuests.Remove(quest.Id);
        //                     Completed?.Invoke(quest);
        //                 }
        //                 else
        //                 {
        //                     Updated?.Invoke(quest);
        //                 }
        //             }
        //             else
        //             {
        //                 Updated?.Invoke(quest);
        //             }
        //         }
        //         else
        //         {
        //             var objective = objectives.FirstOrDefault(o =>
        //                 o.Type is QuestObjective.ObjectiveType.Collect or QuestObjective.ObjectiveType.Give &&
        //                 o.Items.Any(itemGroup => itemGroup.Item.UniqueName == item.Item.UniqueName));
        //
        //             if (objective is null) continue;
        //
        //             var objItem = objective.Items.First(itemGroup => itemGroup.Item.UniqueName == item.Item.UniqueName);
        //
        //             if (objItem.Quantity > item.Quantity)
        //                 objItem.Quantity -= item.Quantity;
        //             else
        //                 objective.Items.Remove(objItem);
        //
        //             if (objective.Items.Count == 0)
        //             {
        //                 objectives.RemoveAt(0);
        //
        //                 if (objectives.Count == 0)
        //                 {
        //                     activeQuests.Remove(quest.Id);
        //                     Completed?.Invoke(quest);
        //                 }
        //                 else
        //                 {
        //                     Updated?.Invoke(quest);
        //                 }
        //             }
        //             else
        //             {
        //                 Updated?.Invoke(quest);
        //             }
        //         }
        //     }
    }
}