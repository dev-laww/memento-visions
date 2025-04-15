using Game.Data;
using Godot;
using Game.Components;
using GodotUtilities;

namespace Game.World;

[Scene]
public partial class Bar : BaseLevel
{
    private Quest quest = QuestRegistry.Get("quest:night_of_shadows");

    [Node] private TransitionArea transitionArea;
    [Node] private ScreenMarker screenMarker;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        base._Ready();
        transitionArea.Toggle(false);
        screenMarker.Toggle(false);
    }

    public void EnableTransitionArea()
    {
        transitionArea.Toggle(true);
        screenMarker.Toggle(true);
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