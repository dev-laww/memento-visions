using Godot;
using Game.Autoload;
using Game.Data;
using GodotUtilities;

namespace Game.Levels.Story;

[Scene]
public partial class EverfieldCity : Node2D
{
    private Quest quest = ResourceLoader.Load<Quest>("res://resources/quests/Chapter1/aswang_hunt.tres");

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        QuestManager.QuestUpdated += OnQuestUpdated;
    }

    private void OnQuestUpdated(Quest updatedQuest)
    {
        if (updatedQuest.ResourcePath != quest.ResourcePath) return;

        if (updatedQuest.Objectives[1].Completed)
        {
            GD.Print("The Missing Child quest is completed!");
        }
    }
}