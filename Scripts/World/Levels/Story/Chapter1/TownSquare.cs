using Godot;
using System;
using Game.Autoload;
using Game.Components;
using Game.Entities;
using Game.UI.Screens;
using GodotUtilities;

namespace Game.Levels.Story;

[Scene]
public partial class TownSquare : Node2D
{
    [Node] private Spawner Spawner;
    [Node] private Entity Chief;
    [Node] private AnimationPlayer AnimationPlayer;
    
    
    
    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated)
        {
            WireNodes();
        }
    }

    public override void _Ready()
    {
        base._Ready();
        InitializeSpawnPoints(); 
       GD.Print("Chief position: ", Chief.Position);
    }
    
    private void InitializeSpawnPoints()
    { 
        Spawner.SpawnPoints.Clear();
        
        Spawner.SpawnPoints.Add(new Vector2(450, 257));
    }
    public async void PlayFadeAnimation()
    {
        AnimationPlayer.Play("Fade");
    }
    public void Spawn()
    {
        Random random = new Random();
        for (int i = 0; i < 8; i++)
        {
            foreach (var enemy in Spawner.Spawn())
            {
                Vector2 offset = new Vector2(random.Next(-3, 4), random.Next(-3, 4));
                enemy.Position += offset;
                AddChild(enemy);
            }
        }
    }
    
    public void RuntoMarker(){}
}