using System.Collections.Generic;
using Godot;

namespace Game.Common.Extensions;

public static class Rect2IExtensions
{
    public static float yMin(this Rect2I rect) => rect.Position.Y;
    public static float yMax(this Rect2I rect) => rect.Position.Y + rect.Size.Y;
    public static float xMin(this Rect2I rect) => rect.Position.X;
    public static float xMax(this Rect2I rect) => rect.Position.X + rect.Size.X;

    public static IEnumerable<Vector2I> AllPositionsWithin(this Rect2I rect)
    {
        for (var y = rect.yMin(); y < rect.yMax(); y++)
        for (var x = rect.xMin(); x < rect.xMax(); x++)
            yield return new Vector2I((int)x, (int)y);
    }
}