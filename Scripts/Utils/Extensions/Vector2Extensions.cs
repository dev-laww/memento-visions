using System;
using Godot;

namespace Game.Utils.Extensions;

public static class Vector2Extensions
{
    public static DelaunayPoint ToPoint(this Vector2 vector) => new(vector.X, vector.Y);

    public static float ManhattanDistanceTo(
        this Vector2 p1,
        Vector2 p2
    ) => Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);

    public static Vector2 SnapToGrid(
        this Vector2 vector,
        int snap = 8
    ) => new(Mathf.Round(vector.X / snap) * snap, Mathf.Round(vector.Y / snap) * snap);
}