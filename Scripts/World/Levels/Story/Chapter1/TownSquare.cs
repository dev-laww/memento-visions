using Game.Components;
using Godot;
using Game.Entities;
using GodotUtilities;

namespace Game.Levels.Story;

[Scene]
public partial class TownSquare : Node2D
{
    [Node] private Entity Mayor;
    [Node] private AnimationPlayer AnimationPlayer;
    [Node] private Spawner Spawner;


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
        Mayor.Visible = false;
        Vector2 specificPosition = new Vector2(437, 259);
        Spawner.SpawnBoss(specificPosition);
    }

    public void RuntoMarker() { }
}