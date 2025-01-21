using Godot;

namespace Game.Components.Battle;

// TODO: implement sound effects, collision shape animations, attack pattern animations
[Tool]
[GlobalClass, Icon("res://assets/icons/weapon-component.svg")]
public partial class WeaponComponent : Node2D
{
    // TODO: Implement this
    [Export] private SpriteFrames Animations { get; set; }
    [Export] private AudioStream AttackSound { get; set; }
    [Export] private AudioStream HitSound { get; set; }

    public AnimationPlayer AnimationPlayer => GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
    public void Animate(string direction) => AnimationPlayer.Play(direction);

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