using Game.Utils.Extensions;

namespace Game.Components;

using Godot;

[Tool]
[GlobalClass]
public partial class SmoothAnimatedSprite2D : AnimatedSprite2D
{
    public override void _Ready() => this.ApplyShader();
}