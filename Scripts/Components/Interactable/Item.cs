using System.Collections.Generic;
using Game.Entities.Player;
using Godot;
using GodotUtilities;
using ItemResource = Game.Resources.Item;

namespace Game.Components.Interactable;

[Scene]
[Tool]
public partial class Item : Node2D
{
    [Export]
    private ItemResource ItemResource
    {
        get => resource;
        set
        {
            resource = value;
            UpdateConfigurationWarnings();
            if (value == null)
            {
                Name = "Item";

                if (sprite != null)
                    sprite.Texture = null;

                if (collision != null)
                    collision.Shape = new CircleShape2D()
                    {
                        Radius = 5
                    };

                return;
            }

            Name = value.Name;

            if (sprite == null || resource.Sprite == null) return;

            sprite.Texture = resource.Sprite;

            if (collision == null) return;

            var size = sprite.Texture.GetSize().X / 2 + 1.4f;

            collision.Shape = new CircleShape2D
            {
                Radius = size
            };
        }
    }

    [Node]
    private Sprite2D sprite;

    [Node]
    private Area2D pickupRange;

    [Node]
    private CollisionShape2D collision;

    private ItemResource resource;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        pickupRange.BodyEntered += body => PickUp(body as Player);
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (ItemResource == null)
            warnings.Add("ItemResource is not set.");

        return warnings.ToArray();
    }

    private void PickUp(Player player)
    {
        if (ItemResource == null)
        {
            GD.PrintErr($"{Name}: ItemResource is not set.");
            return;
        }

        if (player == null)
        {
            GD.PrintErr($"{Name}: Player is null.");
            return;
        }
        
        var tween = CreateTween().SetParallel().SetTrans(Tween.TransitionType.Linear);
        
        tween.TweenProperty(this, "global_position", player.GlobalPosition, 0.2);
        tween.TweenProperty(sprite, "scale", Vector2.Zero, 0.2);

        tween.Finished += QueueFree;
    }
}