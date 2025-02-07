using Godot;
using Game.Components;

namespace Game.World;

public partial class Bar : Node2D
{
    [Export] AudioStream music;

    public override void _Ready()
    {
        var musicManager = GetNode<MusicManager>("/root/MusicManager");
        musicManager.PlayMusic(music);
    }
}