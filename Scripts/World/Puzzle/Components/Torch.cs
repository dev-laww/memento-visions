using Godot;
using Game.Components;
using GodotUtilities;

namespace Game.World.Puzzle;

[Scene]
public partial class Torch : StaticBody2D
{
    [Node] private GpuParticles2D particles;
    private bool isLit = false;

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated && !Engine.IsEditorHint())
            WireNodes();
    }

    public void LightUp(bool lit)
    {
        isLit = lit;
        particles.Emitting = lit;
    }

    public void ResetLight()
    {
        isLit = false;
        particles.Emitting = false;
    }
}