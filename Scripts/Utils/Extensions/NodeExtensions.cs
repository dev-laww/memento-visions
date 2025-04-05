using System.Collections.Generic;
using System.Linq;
using Game.Common.Extensions;
using Game.Components;
using Game.Entities;
using Godot;

namespace Game.Utils.Extensions;

#nullable enable
public static class NodeExtensions
{
    public static Player? GetPlayer(this Node node)
    {
        var player = node.GetTree().GetNodesInGroup("Player").FirstOrDefault();

        return player as Player;
    }

    public static TelegraphCanvas? GetTelegraphCanvas(this Node node) => node.GetTree().Root.GetFirstChildOrNull<TelegraphCanvas>();

    public static IEnumerable<Node> GetNodesInGroup(this Node node, string groupName)
    {
        if (node.IsInGroup(groupName))
        {
            yield return node;
        }

        foreach (Node child in node.GetChildren())
        {
            foreach (var descendant in child.GetNodesInGroup(groupName))
            {
                yield return descendant;
            }
        }
    }

    public static void AddChildren(this Node node, IEnumerable<Node> children)
    {
        children.ToList().ForEach(child =>
        {
            node.AddChild(child);
        });
    }
}