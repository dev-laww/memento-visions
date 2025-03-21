using Godot;
using GodotUtilities;

namespace Game;

[Tool]
[Scene]
public partial class Fog : ParallaxBackground
{
    [Node] private ColorRect colorRect;

    [Export]
    private Color Color
    {
        get => color;
        set
        {
            color = value;

            if (colorRect != null)
            {
                colorRect.Modulate = color;
                colorRect.NotifyPropertyListChanged();
            }
        }
    }

    [Export(PropertyHint.Range, "0,1,0.01")]
    private float Density
    {
        get => density;
        set
        {
            density = value;
            Material?.SetShaderParameter("density", density);
            Material?.NotifyPropertyListChanged();
        }
    }

    [Export]
    private Vector2 Speed
    {
        get => speed;
        set
        {
            speed = value;
            Material?.SetShaderParameter("speed", speed);
            Material?.NotifyPropertyListChanged();
        }
    }



    [Export]
    private NoiseTexture2D Noise
    {
        get => noise;
        set
        {
            noise = value;
            Material?.SetShaderParameter("noise_texture", noise);
            Material?.NotifyPropertyListChanged();
        }
    }

    private float density = 0.4f;
    private Vector2 speed = new(0.02f, 0.01f);
    private Color color = Colors.White;
    private ShaderMaterial Material => colorRect?.Material as ShaderMaterial;
    private NoiseTexture2D noise;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }
}

