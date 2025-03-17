
using Godot;
using GodotUtilities;

namespace Game.Components;

[Tool]
[Scene]
public partial class TelegraphCanvas : CanvasGroup
{
    [Export] private float frequency;

    [Node] private ResourcePreloader resourcePreloader;

    private float time;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        TopLevel = true;
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

