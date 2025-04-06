using Game.Data;
using Godot;
using Game.Components;
using GodotUtilities;

namespace Game.World;

[Scene]
public partial class Bar : BaseLevel
{
    private Quest quest = QuestRegistry.Get("quest:night_of_shadows");

    [Node] private TransitionArea TransitionArea;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        base._Ready();
        TransitionArea.Monitoring = false;
    }

    public void EnableTransitionArea()
    {
        TransitionArea.Monitoring = true;
    }


    public void CompleteObjectiveAtIndex(int index)
    {
        if (quest == null)
        {
            GD.PrintErr("Quest not loaded");
            return;
        }

        quest.CompleteObjective(index);
    }
}