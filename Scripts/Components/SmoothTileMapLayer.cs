using Game.Extensions;
using Godot;

namespace Game.Components;

[Tool]
[GlobalClass]
public partial class SmoothTileMapLayer : TileMapLayer
{
    public override void _Ready() => this.ApplyShader();
}