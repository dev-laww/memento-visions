using System.Linq;
using Game.Entities.Player;
using Godot;
using GodotUtilities;

namespace Game.Utils.Extensions;

public static class Node2DExtensions
{
    public static Player GetPlayer(this Node2D node)
    {
        var player = node.GetTree().GetNodesInGroup<Player>("Player").FirstOrDefault();

        if (player == null)
            GD.PrintErr("Player not found.");

        return player;
    }

    public static void ApplyShader(this Node2D node)
    {
        var material = GD.Load<ShaderMaterial>("res://resources/shaders/smooth-filtering.tres");

        node.TextureFilter = CanvasItem.TextureFilterEnum.Linear;
        node.Material = material;
        node.NotifyPropertyListChanged();
    }
}