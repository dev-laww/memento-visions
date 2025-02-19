using Godot;

namespace Game.UI.Overlays;

// TODO: animate open/close
public abstract partial class Overlay : CanvasLayer
{
    public virtual void Close()
    {
        QueueFree();
    }
}