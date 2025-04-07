using Game.Autoload;
using Game.Common.Extensions;
using Godot;
using Godot.Collections;

namespace Game.Entities;

public abstract partial class Enemy : Entity
{
    public enum EnemyType
    {
        Common,
        Boss
    }

    [Export] public string EnemyName { get; private set; }
    [Export] public EnemyType Type { get; private set; }
    public override string ToString() => $"<Enemy ({Id})>";

    private Tween tween;

    protected override void Die(DeathInfo info)
    {
        if (tween is not null) return;

        SetProcess(false);
        SetPhysicsProcess(false);
        Velocity = Vector2.Zero;

        var shader = new ShaderMaterial
        {
            Shader = ResourceLoader.Load<Shader>("res://resources/shaders/burn_dissolve.gdshader")
        };

        shader.SetShaderParameter("dissolve_texture", new NoiseTexture2D
        {
            Noise = new FastNoiseLite
            {
                NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex,
                Frequency = 0.04f,
            }
        });
        shader.SetShaderParameter("dissolve_value", 1f);
        shader.SetShaderParameter("burn_size", 0.1f);
        shader.SetShaderParameter("burn_color", new Color(0.77f, 0.18f, 0f));

        foreach (var sprite in this.GetAllChildrenOfType<AnimatedSprite2D>())
        {
            sprite.TextureFilter = TextureFilterEnum.Linear;
            sprite.Material = shader;
        }

        tween = CreateTween();
        tween.TweenProperty(shader, "shader_parameter/dissolve_value", 0f, 1f)
            .SetTrans(Tween.TransitionType.Linear)
            .SetEase(Tween.EaseType.Out);
        tween.Chain().TweenCallback(Callable.From(QueueFree));
    }


    public override void _ExitTree()
    {
        if (Engine.IsEditorHint()) return;

        EnemyManager.Unregister(this);
    }
}