using Godot;
using Game.Entities;
using GodotUtilities;

namespace Game.Levels.Story;

[Scene]
public partial class TownSquare : Node2D
{
    [Node] private Entity Chief;
    [Node] private AnimationPlayer AnimationPlayer;



    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated)
        {
            WireNodes();
        }
    }

    public void PlayFadeAnimation()
    {
        AnimationPlayer.Play("Fade");
    }

    public void Spawn()
    {
        // TODO: spawn enemies
    }

    public void RuntoMarker() { }
}