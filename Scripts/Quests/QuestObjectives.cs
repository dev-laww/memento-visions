using System;
using Godot;
using GodotUtilities;


namespace Game.Quests;

[Tool]
[GlobalClass]
public partial class QuestObjectives : Node
{
    public static event Action OnProgressUpdated;
    [Export] public Quest quest { get; set; }
    [Export] public int TargetCount { get; set; } = 1;
    public int currentCount;

    public virtual void StartQuest()
    {
        currentCount = 0;
        quest.Status = Quest.QuestStatus.Active;
    }

    public virtual void ObjectiveComplete()
    {
        quest.CompleteQuest();
    }
    
    public virtual float GetProgress()
    {
        return (float) currentCount / TargetCount;
    }
    
    public virtual void UpdateProgress()
    {
        if (currentCount >= TargetCount)
        {
            ObjectiveComplete();
        }
        OnProgressUpdated?.Invoke();  
    }
    
}