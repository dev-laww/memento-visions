using Godot;
using Game.Components;
using Game.World.Puzzle;
using GodotUtilities;
using Game.Autoload;
using Game.Utils.Extensions;
using Game.Entities;

namespace Game.World;

[Scene]
public partial class DesolatePlace : BaseLevel
{
    [Node] private TransitionArea transitionArea;
    [Node] private TileMapLayer secretDoor;
    [Node] private PressurePlate pressurePlate;
    [Node] private DialogueTrigger door;
    [Node] private AnimationPlayer animationPlayer;
    [Node] private Entity storyTeller;
    [Node] private QuestTrigger photographTrigger;
    [Node] private QuestTrigger chairTrigger;
    [Node] private QuestTrigger diaryTrigger;
    [Node] private ScreenMarker chiefMarker,photoMarker,chairMarker,diaryMarker,transitionMarker;

    private bool isInteracted;
    private bool isDoorTipShown;
    private int objectivesInteracted = 0;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        transitionArea.Toggle(false);
        pressurePlate.Activated += DisableDoor;
        pressurePlate.Deactivated += EnableDoor;
        ToggleMarkers(false);
        transitionMarker.Toggle(false);

    }

    private void DisableDoor()
    {
        if (storyTeller.Visible) return;

        door.Toggle(false);
        CinematicManager.StartCinematic(door.GlobalPosition);
        this.GetPlayer()?.InputManager.AddLock();

        GetTree().CreateTimer(1f).Timeout += () => secretDoor.Enabled = false;

        GetTree().CreateTimer(3f).Timeout += () =>
        {
            this.GetPlayer()?.InputManager.RemoveLock();
            CinematicManager.EndCinematic();
        };
    }

    private void EnableDoor()
    {
        if (storyTeller.Visible) return;

        door.Toggle(!isDoorTipShown);
        CinematicManager.StartCinematic(door.GlobalPosition);
        this.GetPlayer()?.InputManager.AddLock();

        GetTree().CreateTimer(1f).Timeout += () => secretDoor.Enabled = true;

        GetTree().CreateTimer(3f).Timeout += () =>
        {
            this.GetPlayer()?.InputManager.RemoveLock();
            CinematicManager.EndCinematic();
            isDoorTipShown = true;
        };
    }

    public void ToggleMarkers(bool toggle)
    {
        chiefMarker.Toggle(!toggle);
        photoMarker.Toggle(toggle);
        chairMarker.Toggle(toggle);
        diaryMarker.Toggle(toggle);
    }
}