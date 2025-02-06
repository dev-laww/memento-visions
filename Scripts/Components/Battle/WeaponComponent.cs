using System.Collections.Generic;
using Game.Common.Extensions;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.Components.Battle;

// TODO: implement sound effects, collision shape animations, attack pattern animations
[Tool]
[Scene]
[GlobalClass, Icon("res://assets/icons/weapon-component.svg")]
public partial class WeaponComponent : Node2D
{
    [Export]
    private SpriteFrames Animations
    {
        get => animatedSprite2d?.SpriteFrames;
        set
        {
            if (animatedSprite2d == null) return;

            animatedSprite2d.SpriteFrames = value;
            animatedSprite2d.NotifyPropertyListChanged();
        }
    }

    [Export]
    private AudioStream AttackSound
    {
        get => attackSfx?.Stream;
        set
        {
            if (attackSfx == null) return;

            attackSfx.Stream = value;
            attackSfx.NotifyPropertyListChanged();
        }
    }

    [Export]
    private AudioStream HitSound
    {
        get => hitSfx?.Stream;
        set
        {
            if (hitSfx == null) return;

            hitSfx.Stream = value;
            hitSfx.NotifyPropertyListChanged();
        }
    }

    [Node] private SmoothAnimatedSprite2D animatedSprite2d;
    [Node] private AnimationPlayer animationPlayer;
    [Node] private AudioStreamPlayer2D attackSfx;
    [Node] private AudioStreamPlayer2D hitSfx;


    public SignalAwaiter AnimationFinished => ToSignal(animationPlayer, "animation_finished");

    public void Animate() => animationPlayer.Play(this.GetPlayer()?.LastFacedDirection ?? "front");

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (Animations == null)
            warnings.Add("SpriteFrames is not set.");

        if (AttackSound == null)
            warnings.Add("AttackSound is not set.");

        if (HitSound == null)
            warnings.Add("HitSound is not set.");

        return [.. warnings];
    }

    public override void _EnterTree()
    {
        if (GetNodeOrNull("Assets") != null) return;

        var assets = new Node2D { Name = "Assets" };
        var sprite = new SmoothAnimatedSprite2D { Name = "AnimatedSprite2D" };
        var player = new AnimationPlayer { Name = "AnimationPlayer" };
        var atkSfx = new AudioStreamPlayer2D { Name = "AttackSfx" };
        var htSfx = new AudioStreamPlayer2D { Name = "HitSfx" };

        var library = new AnimationLibrary();

        library.AddAnimation("front", new Animation());
        library.AddAnimation("back", new Animation());
        library.AddAnimation("left", new Animation());
        library.AddAnimation("right", new Animation());

        player.AddAnimationLibrary("", library);

        assets.AddChild(sprite);
        assets.AddChild(atkSfx);
        assets.AddChild(htSfx);
        assets.SetDisplayFolded(true);

        sprite.UniqueNameInOwner = true;
        atkSfx.UniqueNameInOwner = true;
        htSfx.UniqueNameInOwner = true;

        this.EditorAddChild(assets);
        this.EditorAddChild(player);
    }
}