using Game.Extensions;
using Godot;

namespace Game.Components;

[Tool]
[GlobalClass]
public partial class SmoothSprite2D : Sprite2D
{
    public override void _Ready() => this.ApplyShader();
}