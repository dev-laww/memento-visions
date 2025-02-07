using System.Collections.Generic;
using Godot;

namespace Game.Components;

[Tool]
[GlobalClass]
public partial class SoundQueue : Node
{
    private int nextSoundIndex = 0;
    private List<AudioStreamPlayer> AudioStreamPlayers = new List<AudioStreamPlayer>();

    [Export] private int Count { get; set; } = 2;

    public override void _Ready()
    {
        var child = GetChild(0);
        if (child is AudioStreamPlayer audioStreamPlayer)
        {
            AudioStreamPlayers.Add(audioStreamPlayer);
            for (int i = 0; i < Count; i++)
            {
                AudioStreamPlayer duplicate = audioStreamPlayer.Duplicate() as AudioStreamPlayer;
                AddChild(duplicate);
                AudioStreamPlayers.Add(duplicate);
            }
        }
    }

    public override string[] _GetConfigurationWarnings()
    {
        if (GetChildCount() == 0)
        {
            return new string[] { "SoundQueue requires at least one AudioStreamPlayer child." };
        }

        if (GetChild(0) is not AudioStreamPlayer)
        {
            return new string[] { "SoundQueue requires AudioStreamPlayer children." };
        }

        return base._GetConfigurationWarnings();
    }
    
    public void PlaySound(AudioStream audio)
    {
        if (!AudioStreamPlayers[nextSoundIndex].IsPlaying())
        {
            AudioStreamPlayers[nextSoundIndex++].Play();
            nextSoundIndex %= AudioStreamPlayers.Count;
        }

    }
}