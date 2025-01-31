using Godot;

namespace Game.Common.Extensions;

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