using Godot;
using System;
using System.Collections.Generic;

namespace Game.Components.Sounds;

[Tool]
[GlobalClass]
public partial class SoundPool : Node
{
    private List<AudioStreamPlayer> players = new();
    private int currentIndex;
    private double lastPlayTime = 0;
    private const double cooldown = 0.05;

    public override void _Ready()
    {
        foreach (Node child in GetChildren())
        {
            if (child is AudioStreamPlayer player)
            {
                players.Add(player);
            }
        }
    }

    public override void _Process(double delta)
    {
        foreach (var player in players)
        {
            if (!player.Playing) player.Stop();
        }
    }

    public void PlaySound(float minPitch = 1.4f, float maxPitch = 2.1f)
    {
        if (players.Count == 0) return;
        
        for (int i = 0; i < players.Count; i++)
        {
            if (!players[currentIndex].Playing)
            {
                var player = players[currentIndex];
                player.PitchScale = (float)GD.RandRange(minPitch, maxPitch);
                player.Play();
                currentIndex = (currentIndex + 1) % players.Count;
                return;
            }

            currentIndex = (currentIndex + 1) % players.Count;
        }
        
        currentIndex = (currentIndex + 1) % players.Count;
        var nextPlayer = players[currentIndex];
        nextPlayer.PitchScale = (float)GD.RandRange(minPitch, maxPitch);
        nextPlayer.Play();
    }



    public void SetVolume(float volumeDb)
    {
        foreach (var player in players)
        {
            player.VolumeDb = volumeDb;
        }
    }

    public void StopAll()
    {
        foreach (var player in players)
        {
            player.Stop();
        }
    }
}