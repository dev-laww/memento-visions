using System.Collections.Generic;
using Game.Entities.Player;
using Godot;
using GodotUtilities;
using ItemResource = Game.Resources.Item;

namespace Game.Components.Interactable;

[Tool]
[Scene]
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

                sprite.Texture = null;
                collision.Shape = new CircleShape2D
                {
                    Radius = 5
                };

                sprite.NotifyPropertyListChanged();
                collision.NotifyPropertyListChanged();

                return;
            }

            Name = string.IsNullOrEmpty(value.Name) ? "Item" : value.Name;

            sprite.Texture = value.Sprite;
            collision.Shape = new CircleShape2D
            {
                Radius = value.Sprite != null ? value.Sprite.GetSize().X / 2 + 1.4f : 5
            };

            sprite.NotifyPropertyListChanged();
            collision.NotifyPropertyListChanged();
        }
    }

    [Node] private Area2D pickupRange;

    private Sprite2D sprite => GetNode<Sprite2D>("Sprite");
    private CollisionShape2D collision => GetNode<CollisionShape2D>("%Collision");
    private ItemResource resource;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready() => pickupRange.BodyEntered += body => PickUp(body as Player);

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
            GD.PrintErr($"{Name}: Body is invalid.");
            return;
        }

        var tween = CreateTween().SetParallel().SetTrans(Tween.TransitionType.Linear);

        tween.TweenProperty(this, "global_position", player.GlobalPosition, 0.2);
        tween.TweenProperty(sprite, "scale", Vector2.Zero, 0.2);
        tween.Finished += () =>
        {
            QueueFree();
            player.Inventory.PickUpItem(ItemResource);
        };
    }
}