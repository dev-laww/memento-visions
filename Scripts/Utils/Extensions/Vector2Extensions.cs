using System;
using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;
using Godot;
using GodotUtilities;

namespace Game.Utils.Extensions;

public static class Vector2Extensions
{
    private static DelaunayPoint ToPoint(this Vector2 vector) => new(vector.X, vector.Y);

    public static float ManhattanDistanceTo(
        this Vector2 p1,
        Vector2 p2
    ) => Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);

    public static Vector2 SnapToGrid(
        this Vector2 vector,
        int snap = 8
    ) => new(Mathf.Round(vector.X / snap) * snap, Mathf.Round(vector.Y / snap) * snap);

    public static List<IEdge> Triangulate(this IEnumerable<Vector2> points)
    {
        var _points = points.Select(p => p.ToPoint()).ToArray<IPoint>();

        var delaunator = new Delaunator(_points);

        return delaunator.GetEdges().ToList();
    }

    public static Vector2I ToVectorI(this Vector2 vector) => new((int)vector.X, (int)vector.Y);

    public static Vector2 ToVector(this Vector2I vector) => new(vector.X, vector.Y);

    public static Vector2I Random(this Vector2I vector, int min = 4, int max = 8)
    {
        var size = MathUtil.RNG.RandiRange(min, max);

        vector.X = size;
        vector.Y = size;

        return vector;
    }

    public static Vector2I Random(this Vector2I vector, Vector2I min, Vector2I max, float aspectRatio = 1.2f)
    {
        vector.X = MathUtil.RNG.RandiRange(min.X, max.X);
        vector.Y = MathUtil.RNG.RandiRange(min.Y, max.Y);

        return vector;
    }
}