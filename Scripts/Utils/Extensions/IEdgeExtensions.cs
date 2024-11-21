using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;

namespace Game.Utils.Extensions;

public static class IEdgeExtensions
{
    public static List<IEdge> MinimumSpanningTree(this IEnumerable<IEdge> edges)
    {
        // Build the adjacency list for the graph
        Dictionary<IPoint, List<IEdge>> graph = new();
        var enumerable = edges as IEdge[] ?? edges.ToArray();
        foreach (var edge in enumerable)
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
        List<IEdge> mst = new();
        var priorityQueue = new SortedSet<(double weight, IEdge edge)>(Comparer<(double, IEdge)>.Create((x, y) =>
        {
            var result = x.Item1.CompareTo(y.Item1);
            return result != 0 ? result : x.Item2.Index.CompareTo(y.Item2.Index);
        }));

        // Start from an arbitrary point (first edge)
        var startPoint = enumerable.First().P;
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
            mst.Add(edge);

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

        return mst;
    }
}