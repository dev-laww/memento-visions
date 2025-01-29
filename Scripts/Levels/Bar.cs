using Godot;
using GodotUtilities;
using Game.Components.Managers;

namespace Game.Levels;

[Scene]
public partial class Bar : Node2D
{
    [Export] AudioStream music;

    public override void _Ready()
    {
        var musicManager = GetNode<MusicManager>("/root/MusicManager");
        musicManager.PlayMusic(music);
    }
}