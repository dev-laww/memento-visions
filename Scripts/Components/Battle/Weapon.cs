using System.Collections.Generic;
using Godot;
using GodotUtilities;
using WeaponResource = Game.Resources.Weapon;

namespace Game.Components.Battle;

// TODO: implement sound effects, collision shape animations, attack pattern animations
[Tool]
[GlobalClass]
public partial class Weapon : Node2D
{
    [Export]
    public WeaponResource Resource
    {
        get => resource;
        set
        {
            resource = value;
            UpdateConfigurationWarnings();

            if (value == null)
            {
                Name = "Weapon";

                if (smoothAnimatedSprite2D != null)
                {
                    smoothAnimatedSprite2D.SpriteFrames = null;
                    smoothAnimatedSprite2D.NotifyPropertyListChanged();
                }

                if (attackSfx != null)
                {
                    attackSfx.Stream = null;
                    attackSfx.NotifyPropertyListChanged();
                }

                if (hitSfx != null)
                {
                    hitSfx.Stream = null;
                    hitSfx.NotifyPropertyListChanged();
                }

                NotifyPropertyListChanged();
                return;
            }

            if (smoothAnimatedSprite2D != null)
            {
                smoothAnimatedSprite2D.SpriteFrames = value.Animations;
                smoothAnimatedSprite2D.NotifyPropertyListChanged();
            }

            if (attackSfx != null)
            {
                attackSfx.Stream = value.AttackSound;
                attackSfx.NotifyPropertyListChanged();
            }

            if (hitSfx != null)
            {
                hitSfx.Stream = value.HitSound;
                hitSfx.NotifyPropertyListChanged();
            }

            Name = string.IsNullOrEmpty(value.Name) ? "Weapon" : value.Name;
            NotifyPropertyListChanged();
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationEnterTree && resource != null)
            Resource = resource;
    }

    private AnimatedSprite2D smoothAnimatedSprite2D => GetNodeOrNull<AnimatedSprite2D>("Assets/SmoothAnimatedSprite2D");
    private AudioStreamPlayer2D attackSfx => GetNodeOrNull<AudioStreamPlayer2D>("Assets/AttackSfx");
    private AudioStreamPlayer2D hitSfx => GetNodeOrNull<AudioStreamPlayer2D>("Assets/HitSfx");
    private AnimationPlayer player => GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
    private WeaponResource resource;

    public SignalAwaiter Animate(string direction)
    {
        player.Play(direction);
        return ToSignal(player, "animation_finished");
    }

    public override void _EnterTree()
    {
        if (GetNodeOrNull("Assets") != null) return;

        var assets = new Node2D();
        var spriteNode = new SmoothAnimatedSprite2D();
        var playerNode = new AnimationPlayer();
        var attackSfxNode = new AudioStreamPlayer2D();
        var hitSfxNode = new AudioStreamPlayer2D();

        assets.Name = "Assets";
        spriteNode.Name = nameof(SmoothAnimatedSprite2D);
        playerNode.Name = nameof(AnimationPlayer);
        attackSfxNode.Name = "AttackSfx";
        hitSfxNode.Name = "HitSfx";

        var library = new AnimationLibrary();

        library.AddAnimation("front", new Animation());
        library.AddAnimation("back", new Animation());
        library.AddAnimation("left", new Animation());
        library.AddAnimation("right", new Animation());

        playerNode.AddAnimationLibrary("", library);

        AddChild(assets);
        AddChild(playerNode);
        assets.AddChild(spriteNode);
        assets.AddChild(attackSfxNode);
        assets.AddChild(hitSfxNode);
        assets.SetDisplayFolded(true);


        spriteNode.SetOwner(GetTree().GetEditedSceneRoot());
        playerNode.SetOwner(GetTree().GetEditedSceneRoot());
        assets.SetOwner(GetTree().GetEditedSceneRoot());
        attackSfxNode.SetOwner(GetTree().GetEditedSceneRoot());
        hitSfxNode.SetOwner(GetTree().GetEditedSceneRoot());
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (resource == null)
            warnings.Add("Resource is not set");

        return warnings.ToArray();
    }
}