using Godot;

namespace Game.Common.Extensions;

public static class AreaExtensions
{
    public static void AddInteractionUI(this Area2D area)
    {
        if (area.GetNodeOrNull("Node2D") != null || !Engine.IsEditorHint()) return;

        var node = new Node2D { Name = "Node2D" };

        var scene = ResourceLoader.Load<PackedScene>("res://Scenes/UI/Common/InteractionUI.tscn");
        var ui = scene.Instantiate(); 

        node.AddChild(ui);
        area.AddChild(node);

        node.SetDisplayFolded(true);
        ui.NotifyPropertyListChanged();
        node.NotifyPropertyListChanged();

        ui.SetOwner(area.GetTree().GetEditedSceneRoot());
        node.SetOwner(area.GetTree().GetEditedSceneRoot());
    }
}