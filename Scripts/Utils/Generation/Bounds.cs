using Godot;

namespace Game.Utils.Generation;

public class Bounds
{
    public Rect2I Rect { get; }

    public Vector2I Center => Rect.Position + Rect.Size / 2;

    public Bounds(Vector2I position, Vector2I size)
    {
        Rect = new Rect2I(position, size);
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