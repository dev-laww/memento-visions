using DelaunatorSharp;
using Godot;

namespace Game.Extensions;

public static class IPointExtensions
{
    public static Vector2 ToVector(this IPoint point) => new((float)point.X, (float)point.Y);

    public static Vector2I ToVectorI(this IPoint point) => new((int)point.X, (int)point.Y);
}