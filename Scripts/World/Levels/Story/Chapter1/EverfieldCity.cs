using Godot;
using Game.Autoload;
using Game.Data;
using Game.Entities;
using GodotUtilities;

namespace Game.Levels.Story;

[Scene]
public partial class EverfieldCity : Node2D
{
    [Node] private Entity Chief2;
    private Quest quest = ResourceLoader.Load<Quest>("res://resources/quests/Chapter1/aswang-hunt2.tres");
    private Quest quest1 = ResourceLoader.Load<Quest>("res://resources/quests/Chapter1/whisper-intramuros1.tres");

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        
        QuestManager.Instance.QuestUpdated += OnQuestUpdated;
        GD.Print($"instance {QuestManager.Instance.Name}" );

    }

    private void OnQuestUpdated(Quest updatedQuest)
    {
        if (updatedQuest == null)
        {
            GD.PrintErr("Updated quest is null!");
            return;
        }

        if (updatedQuest.ResourcePath != quest.ResourcePath) return;

        if (updatedQuest.Objectives == null || updatedQuest.Objectives.Count <= 1)
        {
            GD.PrintErr("Objectives are null or not enough objectives!");
            return;
        }
      
        if (updatedQuest.Objectives[0].Completed && !Chief2.Visible)
        {
            Chief2.Visible = true;
        }
    }
    
    public void CompleteObjectiveAtIndex(int index)
    {
        if (quest1 == null)
        {
            GD.PrintErr("Quest not loaded");
            return;
        }
        
        quest1.CompleteObjective(index);
    }
}