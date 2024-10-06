using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.Utils;

public class DelaunayPoint : IPoint
{
    public double X { get; set; }
    public double Y { get; set; }

    public DelaunayPoint(double x, double y)
    {
        X = x;
        Y = y;
    }
}

public class Generator
{
    public static List<Vector2> GeneratePoints(
        int count,
        float radius = 200f,
        int tileSize = 32,
        float proximityThreshold = 50f
    )
    {
        HashSet<Vector2> usedPositions = new();
        List<Vector2> points = new();

        for (var i = 0; i < count; i++)
        {
            Vector2 position;

            do
            {
                position = new Vector2(
                    MathUtil.RNG.RandfRange(-radius, radius),
                    MathUtil.RNG.RandfRange(-radius, radius)
                );

                var offsetX = MathUtil.RNG.RandfRange(-tileSize, tileSize);
                var offsetY = MathUtil.RNG.RandfRange(-tileSize, tileSize);
                position += new Vector2(offsetX, offsetY);
            } while (usedPositions.Any(usedPos => position.DistanceTo(usedPos) < proximityThreshold));

            position = position.SnapToGrid();
            usedPositions.Add(position);
            points.Add(position); // Add the generated position to the points list
        }

        return points;
    }

    public static List<IEdge> MinimumSpanningTree(List<IEdge> edges)
    {
        // Build the adjacency list for the graph
        Dictionary<IPoint, List<IEdge>> graph = new();
        foreach (var edge in edges)
        {
            if (!graph.ContainsKey(edge.P))
                graph[edge.P] = new List<IEdge>();

            if (!graph.ContainsKey(edge.Q))
                graph[edge.Q] = new List<IEdge>();

            graph[edge.P].Add(edge);
            graph[edge.Q].Add(edge);
        }

        // Prim's algorithm to find MST with Manhattan distance
        HashSet<IPoint> visited = new();
        List<IEdge> mstEdges = new();
        var priorityQueue = new SortedSet<(double weight, IEdge edge)>(Comparer<(double, IEdge)>.Create((x, y) =>
        {
            var result = x.Item1.CompareTo(y.Item1);
            return result != 0 ? result : x.Item2.Index.CompareTo(y.Item2.Index);
        }));

        // Start from an arbitrary point (first edge)
        var startPoint = edges.First().P;
        visited.Add(startPoint);

        // Initialize the priority queue with edges from the start point
        foreach (var edge in graph[startPoint])
        {
            var (p, q) = (edge.P.ToVector(), edge.Q.ToVector());
            var weight = p.ManhattanDistanceTo(q); // Use Manhattan distance
            priorityQueue.Add((weight, edge));
        }

        // While there are edges in the priority queue
        while (priorityQueue.Count > 0)
        {
            var (_, edge) = priorityQueue.Min;
            priorityQueue.Remove(priorityQueue.Min);

            // If both points of the edge are visited, skip it
            if (visited.Contains(edge.P) && visited.Contains(edge.Q)) continue;

            // Add edge to MST
            mstEdges.Add(edge);

            // Add the newly visited vertex to the visited set
            var newPoint = visited.Contains(edge.P) ? edge.Q : edge.P;
            visited.Add(newPoint);

            // Add edges of the newly visited vertex to the priority queue
            foreach (
                var newEdge in graph[newPoint].Where(
                    newEdge => !visited.Contains(newEdge.P) || !visited.Contains(newEdge.Q)
                )
            )
            {
                var (p, q) = (newEdge.P.ToVector(), newEdge.Q.ToVector());
                var newWeight = p.ManhattanDistanceTo(q); // Use Manhattan distance
                priorityQueue.Add((newWeight, newEdge));
            }
        }

        return mstEdges;
    }

    public static List<IEdge> TriangulatePoints(List<Vector2> points)
    {
        var _points = points.Select(p => p.ToPoint()).ToArray<IPoint>();

        var delaunator = new Delaunator(_points);

        return delaunator.GetEdges().ToList();
    }
}