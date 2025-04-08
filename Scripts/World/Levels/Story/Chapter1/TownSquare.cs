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
    }

    public void Spawn()
    {
        mayor.Visible = false;

        for (var i = 0; i < 5; i++)
        {
            var aswang = resourcePreloader.InstanceSceneOrNull<Aswang>();
            aswang.GlobalPosition = mayor.GlobalPosition + new Vector2(0, 100) * MathUtil.RNG.RandDirection();

            AddChild(aswang);
        }

        var aghon = resourcePreloader.InstanceSceneOrNull<Aghon>();
        aghon.GlobalPosition = mayor.GlobalPosition;
        aghon.Death += _ => SaveManager.UnlockFrenzyMode();

        AddChild(aghon);
    }

    public void RuntoMarker() { }
}