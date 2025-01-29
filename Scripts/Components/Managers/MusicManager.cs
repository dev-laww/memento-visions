using Godot;
using System.Collections.Generic;
using Game.Resources;
using GodotUtilities;

namespace Game.Components.Managers;

[GlobalClass]
public partial class MusicManager : Node2D
{
    private int currentAudioPlayerIndex = 0;
    private List<AudioStreamPlayer2D> musicAudioPlayers = new List<AudioStreamPlayer2D>();
    private string musicBus = "Music";
    private float musicFadeDuration = 0.5f;

    public override void _Ready()
    {
        SetProcess(true);
        for (int i = 0; i < 2; i++)
        {
            var audioPlayer = new AudioStreamPlayer2D();
            AddChild(audioPlayer);
            audioPlayer.Bus = musicBus;
            musicAudioPlayers.Add(audioPlayer);
            audioPlayer.VolumeDb = -40;
        }
    }

    public void PlayMusic(AudioStream audio)
    {
        if (musicAudioPlayers.Count == 0) return;


        if (audio == musicAudioPlayers[currentAudioPlayerIndex].Stream &&
            musicAudioPlayers[currentAudioPlayerIndex].Playing)
            return;


        if (musicAudioPlayers[currentAudioPlayerIndex].Playing)
        {
            FadeOutAndStop(musicAudioPlayers[currentAudioPlayerIndex]);
        }


        currentAudioPlayerIndex = (currentAudioPlayerIndex + 1) % musicAudioPlayers.Count;


        var currentAudioPlayer = musicAudioPlayers[currentAudioPlayerIndex];
        currentAudioPlayer.Stream = audio;
        PlayAndFadeIn(currentAudioPlayer);
    }

    private void PlayAndFadeIn(AudioStreamPlayer2D player)
    {
        player.VolumeDb = -80;
        player.Play();

        var tween = CreateTween();
        tween.TweenProperty(player, "volume_db", -20, musicFadeDuration);
    }

    private void FadeOutAndStop(AudioStreamPlayer2D player)
    {
        var tween = CreateTween();
        tween.TweenProperty(player, "volume_db", -80, musicFadeDuration);
        tween.TweenCallback(Callable.From(() => player.Stop()));
    }
}