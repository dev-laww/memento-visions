using Godot;
using System.Collections.Generic;

namespace Game.Components;

[Tool]
public partial class SoundManager : Node
{
    public static SoundManager Instance { get; private set; }
    
    [Export] public AudioStream[] SoundEffects { get; private set; }
    [Export] public string[] SoundNames { get; private set; }
    
    private Dictionary<string, SoundPool> soundPools = new();
    
    // Configuration constants
    private const int DEFAULT_POOL_SIZE = 3;
    private const float DEFAULT_VOLUME_DB = 0.0f;
    
    public override void _Ready()
    {
        if (Instance != null && Instance != this)
        {
            QueueFree();
            return;
        }
        Instance = this;
        
        InitializeSoundPools();
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();
        
        if (SoundEffects == null || SoundNames == null)
        {
            warnings.Add("Both SoundEffects and SoundNames arrays must be set.");
            return warnings.ToArray();
        }

        if (SoundEffects.Length != SoundNames.Length)
        {
            warnings.Add("SoundEffects and SoundNames arrays must be the same length.");
        }

        if (SoundNames != null)
        {
            var uniqueNames = new HashSet<string>();
            for (int i = 0; i < SoundNames.Length; i++)
            {
                if (string.IsNullOrEmpty(SoundNames[i]))
                {
                    warnings.Add($"Sound name at index {i} is empty.");
                    continue;
                }
                
                if (!uniqueNames.Add(SoundNames[i]))
                {
                    warnings.Add($"Duplicate sound name found: {SoundNames[i]}");
                }
            }
        }

        return warnings.ToArray();
    }
    
    private void InitializeSoundPools()
    {
        if (SoundEffects == null || SoundNames == null || 
            SoundEffects.Length != SoundNames.Length) return;

        for (int i = 0; i < SoundEffects.Length; i++)
        {
            if (SoundEffects[i] != null && !string.IsNullOrEmpty(SoundNames[i]))
            {
                CreateSoundPool(SoundNames[i], SoundEffects[i]);
            }
        }
    }
    
    private void CreateSoundPool(string soundId, AudioStream audioStream, int poolSize = DEFAULT_POOL_SIZE)
    {
        var pool = new SoundPool();
        
        for (int i = 0; i < poolSize; i++)
        {
            var player = new AudioStreamPlayer
            {
                Stream = audioStream,
                VolumeDb = DEFAULT_VOLUME_DB,
                Name = $"{soundId}_Player_{i}",
                Bus ="SFX"
            };
            pool.AddChild(player);
        }
        
        AddChild(pool);
        soundPools[soundId] = pool;
    }
    
    public void PlaySound(string soundId)
    {
        if (!soundPools.ContainsKey(soundId))
        {
            GD.PrintErr($"Sound '{soundId}' not found in sound pools.");
            return;
        }
        
        soundPools[soundId].PlaySound();
    }
    
    public void SetVolume(string soundId, float volumeDb)
    {
        if (soundPools.TryGetValue(soundId, out var pool))
        {
            pool.SetVolume(volumeDb);
        }
    }
    public void StopSound(string soundId)
    {
        if (soundPools.TryGetValue(soundId, out var pool))
        {
            pool.StopAll();
        }
        else
        {
            GD.PrintErr($"Sound not found: {soundId}");
        }
    }
    
    public void StopAll()
    {
        foreach (var pool in soundPools.Values)
        {
            pool.StopAll();
        }
    }
}