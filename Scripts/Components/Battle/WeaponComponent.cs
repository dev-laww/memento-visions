using System;
using System.Collections.Generic;
using System.Linq;
using Game.Common.Extensions;
using Godot;
using GodotUtilities;

namespace Game.Components;

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

    public event Action AnimationFinished;

    [Node] private SmoothAnimatedSprite2D animatedSprite2d;
    [Node] private AnimationTree animationTree;
    [Node] private AudioStreamPlayer2D attackSfx;
    [Node] private AudioStreamPlayer2D hitSfx;

    private AnimationNodeStateMachinePlayback playback;
    private int combo = 1;
    private Vector2 blendPosition;

    public void Animate(int combo)
    {
        this.combo = combo;

        playback.Start("animate");
    }

    public void SetBlendPosition(Vector2 position)
    {
        blendPosition = position;
        animationTree.Set("parameters/animate/blend_position", blendPosition);
    }

    public void PlayAttackSound()
    {
        if (attackSfx?.Stream != null)
        {
            attackSfx.Play();
        }
    }

    public override void _Ready()
    {
        var hitBoxes = GetChildren().OfType<HitBox>();

        if (Engine.IsEditorHint()) return;

        playback = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");
        animationTree.AnimationFinished += _ => AnimationFinished?.Invoke();
    }

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
        var animationTree = new AnimationTree
        {
            Name = "AnimationTree",
            TreeRoot = new AnimationNodeStateMachine(),
        };

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
        this.EditorAddChild(animationTree);
    }
}
