
using Godot;
using GodotUtilities;

namespace Game;

[Scene]
[Tool]
public partial class TelegraphCanvas : CanvasGroup
{
    [Export] private float frequency;

    private float time;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Process(double delta)
    {

        time += (float)delta;

        var color = (Material as ShaderMaterial)?.GetShaderParameter("color").As<Color>() ?? Colors.Red;

        var t = (Mathf.Sin(time * frequency) + 1) / 2;

        color = new Color(color.R, t, t, color.A);

        (Material as ShaderMaterial)?.SetShaderParameter("color", color);
    }
}

