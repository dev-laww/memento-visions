using Godot;

namespace Game.Utils.Dungeon;

public class Grid<T>
{
    private T[] data;

    public Vector2I Size { get; private set; }
    public Vector2I Offset { get; set; }

    public Grid(Vector2I size, Vector2I offset)
    {
        Size = size;
        Offset = offset;
        data = new T[size.X * size.Y];
    }

    public int GetIndex(Vector2I pos)
    {
        return pos.X + (Size.X * pos.Y);
    }

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