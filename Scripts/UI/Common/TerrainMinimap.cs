using System.Collections.Generic;
using System.Linq;
using Game.Autoload;
using Game.Common.Extensions;
using Game.Components;
using Game.Entities;
using Game.Utils.Extensions;
using Game.World;
using Godot;
using GodotUtilities;

namespace Game;

[Scene]
public partial class TerrainMinimap : SubViewportContainer
{
    [Node] private SubViewport subViewport;
    [Node] private Camera2D camera2D;
    [Node] private Node2D map;

    private Sprite2D player;
    private readonly Dictionary<Enemy, Sprite2D> enemySprites = [];

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override async void _Ready()
    {
        await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

        player = new Sprite2D { Texture = new PlaceholderTexture2D { Size = new Vector2(24, 24) } };

        var currentScene = GameManager.CurrentScene;
        var tileMapLayers = currentScene.GetAllChildrenOfType<TileMapLayer>().Select(x => x.Duplicate());
        var chests = currentScene.GetAllChildrenOfType<Chest>().ToList();

        chests.ForEach(chest =>
        {
            var chestSprite = new Sprite2D
            {
                Texture = new PlaceholderTexture2D { Size = new Vector2(24, 24) },
                GlobalPosition = chest.GlobalPosition
            };

            map.AddChild(chestSprite);
            chest.TreeExited += () => chestSprite.QueueFree();
        });

        EnemyManager.EnemyRegistered += OnEnemyRegistered;
        EnemyManager.EnemyUnregistered += OnEnemyUnregistered;
        EnemyManager.Enemies.ToList().ForEach(enemy =>
        {
            var enemySprite = new Sprite2D
            {
                Texture = new PlaceholderTexture2D { Size = new Vector2(24, 24) },
                GlobalPosition = enemy.GlobalPosition
            };

            map.AddChild(enemySprite);
            enemy.TreeExited += () => enemySprite.QueueFree();
        });

        map.AddChild(player);
        map.AddChildren(tileMapLayers);
    }

    public override void _Process(double delta)
    {
        try
        {
            var actualPlayer = this.GetPlayer();

            if (actualPlayer == null) return;

            camera2D.GlobalPosition = actualPlayer.GlobalPosition;
            player.GlobalPosition = actualPlayer.Center.GlobalPosition;

            foreach (var (enemy, sprite) in enemySprites)
            {
                sprite.GlobalPosition = enemy.GlobalPosition;
            }
        }
        catch { }
    }

    public void OnEnemyRegistered(Enemy enemy)
    {
        var enemySprite = new Sprite2D
        {
            Texture = new PlaceholderTexture2D { Size = new Vector2(24, 24) },
            GlobalPosition = enemy.GlobalPosition
        };

        map.AddChild(enemySprite);
        enemySprites[enemy] = enemySprite;

        enemy.TreeExited += () => OnEnemyUnregistered(enemy);
    }

    public void OnEnemyUnregistered(Enemy enemy)
    {
        if (enemySprites.TryGetValue(enemy, out var enemySprite))
        {
            enemySprite.QueueFree();
            enemySprites.Remove(enemy);
        }
    }
}

