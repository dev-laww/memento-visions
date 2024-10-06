using DelaunatorSharp;
using Godot;

namespace Game.Utils.Extensions;

public static class IPointExtensions
{
    public static Vector2 ToVector(this IPoint point) => new((float)point.X, (float)point.Y);
}