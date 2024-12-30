using System.Linq;
using Game.Entities.Player;
using Godot;
using GodotUtilities;

namespace Game.Utils.Extensions;

#nullable enable
public static class Node2DExtensions
{
    public static void ApplyShader(this Node2D node)
    {
        var material = GD.Load<ShaderMaterial>("res://resources/shaders/smooth-filtering.tres");

        node.TextureFilter = CanvasItem.TextureFilterEnum.Linear;
        node.Material = material;
        node.NotifyPropertyListChanged();
    }
}