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

    public static T? GetFirstChildOrNull<T>(this Node node) where T : Node
    {
        if (node is T t) return t;

        foreach (Node child in node.GetChildren())
        {
            T? found = GetFirstChildOrNull<T>(child);

            if (found != null) return found;
        }

        return null;
    }

    public static IEnumerable<T> GetAllChildrenOfType<T>(this Node node) where T : Node
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is T t)
                yield return t;

            foreach (T descendant in child.GetAllChildrenOfType<T>())
            {
                yield return descendant;
            }
        }
    }

    public static T Duplicate<T>(this T node) where T : Node
    {
        var duplicate = node.Duplicate() as T ?? throw new InvalidOperationException($"Failed to duplicate node of type {typeof(T)}");

        return duplicate;
    }
}