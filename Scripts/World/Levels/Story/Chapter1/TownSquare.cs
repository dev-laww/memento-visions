using Game.Autoload;
using Game.Entities;
using Godot;
using GodotUtilities;

namespace Game.World;

[Scene]
public partial class TownSquare : BaseLevel
{
    [Node] private Entity mayor;
    [Node] private ResourcePreloader resourcePreloader;
    [Node] private AnimationPlayer animationPlayer;
    [Node] private AudioStreamPlayer2D bossBgm;
    [Node] private AudioStreamPlayer2D chapter1Bgm;

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated)
        {
            WireNodes();
        }
    }

    public void PlayFadeAnimation()
    {
        animationPlayer.Play("Fade");
        chapter1Bgm.StreamPaused = true;
        bossBgm.StreamPaused = false;

    }

    public void Spawn()
    {
        mayor.Visible = false;
        var aghon = resourcePreloader.InstanceSceneOrNull<Aghon>();
        aghon.GlobalPosition = mayor.GlobalPosition;
        aghon.Death += _ => SaveManager.UnlockFrenzyMode();

        AddChild(aghon);
    }
    
}