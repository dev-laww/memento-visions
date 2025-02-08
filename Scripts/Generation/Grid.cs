using Godot;

namespace Game.Generation;

public class Grid<T>(Vector2I size, Vector2I offset)
{
    private readonly T[] data = new T[size.X * size.Y];

    public Vector2I Size { get; private set; } = size;
    public Vector2I Offset { get; set; } = offset;

    public int GetIndex(Vector2I pos) => pos.X + Size.X * pos.Y;

    public bool InBounds(Vector2I pos)
    {
        var rect = new Rect2I(Vector2I.Zero, Size);
        return rect.HasPoint(pos + Offset);
    }

    public T this[int x, int y]
    {
        get => this[new Vector2I(x, y)];
        set => this[new Vector2I(x, y)] = value;
    }

    public T this[Vector2I pos]
    {
        get
        {
            pos += Offset;
            return data[GetIndex(pos)];
        }
        set
        {
            pos += Offset;
            data[GetIndex(pos)] = value;
        }
    }
}