using System;
using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;
using Godot;

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
}