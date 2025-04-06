using Godot;
using Game.Components;
using Game.Entities;
using GodotUtilities;

namespace Game.World;

[Scene]
public partial class SchoolOutdoor : BaseLevel
{
    [Node] private Entity StoryTeller;
    public int ObjectiveInteracted = 0;
    public bool isStoryTellerVisible = true;
    [Node] TransitionArea TransitionArea;
    [Node] private DialogueTrigger DialoguePrologue;


    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public void setDialoguePrologueOff()
    {
        DialoguePrologue.Monitoring = false;
    }


    public void setStoryTellerVisible()
    {
        ((StoryTeller)StoryTeller).Work();
        StoryTeller.Visible = true;
        isStoryTellerVisible = true;
    }
}