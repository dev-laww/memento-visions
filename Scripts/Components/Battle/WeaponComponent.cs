using Game.Resources;
using Godot;

namespace Game.Components.Battle;

// TODO: implement sound effects, collision shape animations, attack pattern animations
[Tool]
[GlobalClass]
public partial class WeaponComponent : Node2D
{
    [Export] private Weapon Weapon;
    [Export] private SpriteFrames Animations { get; set; }
    [Export] private AudioStream AttackSound { get; set; }
    [Export] private AudioStream HitSound { get; set; }

    private AnimatedSprite2D smoothAnimatedSprite2D => GetNodeOrNull<AnimatedSprite2D>("Assets/SmoothAnimatedSprite2D");
    private AudioStreamPlayer2D attackSfx => GetNodeOrNull<AudioStreamPlayer2D>("Assets/AttackSfx");
    private AudioStreamPlayer2D hitSfx => GetNodeOrNull<AudioStreamPlayer2D>("Assets/HitSfx");
    private AnimationPlayer player => GetNodeOrNull<AnimationPlayer>("AnimationPlayer");

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
}