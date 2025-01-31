using Godot;

namespace Game.Common.Extensions;

public static class AreaExtensions
{
    public static void AddInteractionUI(this Area2D area)
    {
        var node = new Node2D { Name = "Node2D" };

        var collision = new CollisionShape2D
        {
            Name = "CollisionShape2D",
            DebugColor = new Color(0.88f, 0.525f, 0.898f, 0.42f)
        };
        var scene = ResourceLoader.Load<PackedScene>("res://Scenes/UI/Common/InteractionUI.tscn");
        var ui = scene.Instantiate();
        ui.UniqueNameInOwner = true;

        node.AddChild(ui);

        area.AddChild(collision);
        area.AddChild(node);

        node.SetDisplayFolded(true);
        collision.NotifyPropertyListChanged();
        ui.NotifyPropertyListChanged();
        node.NotifyPropertyListChanged();

        collision.SetOwner(area.GetTree().GetEditedSceneRoot());
        ui.SetOwner(area.GetTree().GetEditedSceneRoot());
        node.SetOwner(area.GetTree().GetEditedSceneRoot());
    }
}