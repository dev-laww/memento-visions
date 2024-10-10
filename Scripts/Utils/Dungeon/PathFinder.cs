using Godot;
using System;
using System.Collections.Generic;
using Priority_Queue;

namespace Game.Utils.Dungeon;

public class PathFinder
{
    public class Node
    {
        public Vector2I Position { get; }
        public Node Previous { get; set; }
        public float Cost { get; set; }

        public Node(Vector2I position)
        {
            Position = position;
        }
    }

    public struct PathCost
    {
        public bool Traversable;
        public float Cost;
    }

    private static readonly Vector2I[] Neighbors =
    {
        new(1, 0),
        new(-1, 0),
        new(0, 1),
        new(0, -1),
    };

    private readonly Dictionary<Vector2I, Node> grid;
    private readonly SimplePriorityQueue<Node, float> queue;
    private readonly HashSet<Node> closed;
    private readonly Stack<Vector2I> stack;
    private readonly Vector2I gridSize;

    public PathFinder(Vector2I size)
    {
        gridSize = size;
        grid = new Dictionary<Vector2I, Node>();

        queue = new SimplePriorityQueue<Node, float>();
        closed = new HashSet<Node>();
        stack = new Stack<Vector2I>();

        for (var x = 0; x < size.X; x++)
        {
            for (var y = 0; y < size.Y; y++)
            {
                var position = new Vector2I(x, y);
                grid[position] = new Node(position);
            }
        }
    }

    private void ResetNodes()
    {
        foreach (var node in grid.Values)
        {
            node.Previous = null;
            node.Cost = float.PositiveInfinity;
        }
    }

    public List<Vector2I> FindPath(Vector2I start, Vector2I end, Func<Node, Node, PathCost> costFunction)
    {
        ResetNodes();
        queue.Clear();
        closed.Clear();

        var startNode = grid[start];
        startNode.Cost = 0;
        queue.Enqueue(startNode, 0);

        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();
            closed.Add(currentNode);

            if (currentNode.Position == end)
            {
                return ReconstructPath(currentNode);
            }

            foreach (var offset in Neighbors)
            {
                var neighborPos = currentNode.Position + offset;
                if (!InBounds(neighborPos)) continue;

                var neighbor = grid[neighborPos];
                if (closed.Contains(neighbor)) continue;

                var pathCost = costFunction(currentNode, neighbor);
                if (!pathCost.Traversable) continue;

                var newCost = currentNode.Cost + pathCost.Cost;

                if (!(newCost < neighbor.Cost)) continue;
                neighbor.Previous = currentNode;
                neighbor.Cost = newCost;

                if (queue.Contains(neighbor))
                    queue.UpdatePriority(neighbor, newCost);
                else
                    queue.Enqueue(neighbor, newCost);
            }
        }

        return null;
    }

    private bool InBounds(Vector2I position)
    {
        return position.X >= 0 && position.X < gridSize.X && position.Y >= 0 && position.Y < gridSize.Y;
    }

    private List<Vector2I> ReconstructPath(Node node)
    {
        var result = new List<Vector2I>();

        while (node != null)
        {
            stack.Push(node.Position);
            node = node.Previous;
        }

        while (stack.Count > 0)
        {
            result.Add(stack.Pop());
        }

        return result;
    }
}