using System.Collections.Generic;
using Godot;

namespace Game.Generation.Dungeon;

public class Bounds
{
    public Rect2I Rect { get; }

    public Vector2I Center => Rect.Position + Rect.Size / 2;
    
    public List<EntryPoint> EntryPoints { get; } = new();

    public Bounds(Vector2I position, Vector2I size)
    {
        Rect = new Rect2I(position, size);
        
        var edges = new List<(Vector2I, Vector2I)>
        {
            (new Vector2I(Rect.Position.X + Rect.Size.X / 2, Rect.Position.Y + Rect.Size.Y), Vector2I.Up),
            (new Vector2I(Rect.Position.X + Rect.Size.X / 2, Rect.Position.Y), Vector2I.Down),
            (new Vector2I(Rect.Position.X, Rect.Position.Y + Rect.Size.Y / 2), Vector2I.Left),
            (new Vector2I(Rect.Position.X + Rect.Size.X, Rect.Position.Y + Rect.Size.Y / 2), Vector2I.Right),
        };

        edges.ForEach(edge => EntryPoints.Add(EntryPoint.Create(edge.Item1, edge.Item2)));
    }

    public bool Intersects(Bounds other, int padding = 0)
    {
        var thisXMin = Rect.Position.X - padding;
        var thisXMax = Rect.Position.X + Rect.Size.X + padding;
        var thisYMin = Rect.Position.Y - padding;
        var thisYMax = Rect.Position.Y + Rect.Size.Y + padding;

        var otherXMin = other.Rect.Position.X;
        var otherXMax = other.Rect.Position.X + other.Rect.Size.X;
        var otherYMin = other.Rect.Position.Y;
        var otherYMax = other.Rect.Position.Y + other.Rect.Size.Y;

        return !(thisXMax <= otherXMin || thisXMin >= otherXMax || thisYMax <= otherYMin || thisYMin >= otherYMax);
    }
}