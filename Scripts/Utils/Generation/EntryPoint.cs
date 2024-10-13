using Godot;

namespace Game.Utils.Generation;

public class EntryPoint
{
    public Vector2I Position { get; }
    public Vector2I Direction { get; }
    public bool Open { get; private set; }

    public EntryPoint(Vector2I position, Vector2I direction)
    {
        Position = position;
        Direction = direction;
    }

    public void Toggle() => Open = !Open;

    public static EntryPoint Create(Vector2I position, Vector2I direction) => new(position, direction);
}
