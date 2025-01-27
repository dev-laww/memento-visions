using Godot;

namespace Game.Resources;

[Tool]
[GlobalClass, Icon("res://assets/icons/enemy.svg")]
public partial class Enemy : Resource
{
    public enum EnemyType
    {
        Common,
        Boss
    }

    [Export] public string Id;
    [Export] public PackedScene Scene;
    [Export] public EnemyType Type;

    public Game.Entities.Enemies.Enemy Instance => Scene.Instantiate() as Game.Entities.Enemies.Enemy;
}