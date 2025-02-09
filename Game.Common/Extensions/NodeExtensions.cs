using Godot;

namespace Game.Common.Extensions;

public static class NodeExtensions
{
    public static void EditorAddChild(this Node parent, Node child)
    {
        parent.AddChild(child);

        child.SetOwner(parent.GetTree().GetEditedSceneRoot());
        child.SetDisplayFolded(true);
        child.GetChildren().ToList().ForEach(c => c.SetOwner(parent.GetTree().GetEditedSceneRoot()));

        parent.NotifyPropertyListChanged();
        child.NotifyPropertyListChanged();
    }
}