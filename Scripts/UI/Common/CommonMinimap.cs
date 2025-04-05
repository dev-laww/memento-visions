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
public partial class CommonMinimap : Control
{
    [Node] private Sprite2D player;
    [Node] private Sprite2D commonEnemy;
    [Node] private Sprite2D bossEnemy;
    [Node] private Sprite2D chest;
    [Node] private TextureRect map;

    private Vector2 mapScale;
    private float zoom = 1.5f;

    private readonly Dictionary<Node2D, Sprite2D> sprites = [];

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        player.Position = map.GetRect().Size / 2;
        mapScale = map.GetRect().Size / (GetViewportRect().Size / zoom);

        var currentScene = GameManager.CurrentScene;
        var chests = currentScene.GetAllChildrenOfType<Chest>().ToList();

        chests.ForEach(chest =>
        {
            var chestSprite = this.chest.Duplicate() as Sprite2D;

            sprites[chest] = chestSprite;
            map.AddChild(chestSprite);
            chestSprite.Show();
            chest.TreeExited += () =>
            {
                if (!sprites.TryGetValue(chest, out var chestSprite)) return;

                chestSprite.QueueFree();
                sprites.Remove(chest);
            };
        });

        EnemyManager.EnemyRegistered += OnEnemyRegistered;
        EnemyManager.EnemyUnregistered += OnEnemyUnregistered;
        EnemyManager.Enemies.ToList().ForEach(OnEnemyRegistered);
    }

    public override void _Process(double delta)
    {
        var actualPlayer = this.GetPlayer();

        player.Rotation = actualPlayer.VelocityManager.LastFacedDirection.Angle() + Mathf.Pi / 2;

        foreach (var (obj, sprite) in sprites)
        {
            var objPosition = (obj.Position - actualPlayer.Position) * mapScale + map.GetRect().Size / 2;
            sprite.Position = new Vector2(
                Mathf.Clamp(objPosition.X, 0, map.GetRect().Size.X),
                Mathf.Clamp(objPosition.Y, 0, map.GetRect().Size.Y)
            );
        }
    }

    public void OnEnemyRegistered(Enemy enemy)
    {
        try
        {
            var enemySprite = enemy.Type switch
            {
                Enemy.EnemyType.Common => commonEnemy.Duplicate() as Sprite2D,
                Enemy.EnemyType.Boss => bossEnemy.Duplicate() as Sprite2D,
                _ => commonEnemy.Duplicate() as Sprite2D
            };

            enemySprite.Show();
            map.AddChild(enemySprite);
            sprites[enemy] = enemySprite;
            enemy.TreeExited += () => OnEnemyUnregistered(enemy);
        }
        catch { }
    }

    public void OnEnemyUnregistered(Enemy enemy)
    {
        if (!sprites.TryGetValue(enemy, out var enemySprite)) return;

        enemySprite.QueueFree();
        sprites.Remove(enemy);
    }
}

