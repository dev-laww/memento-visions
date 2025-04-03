using System.Linq;
using Game.Common;
using Game.Entities;
using GodotUtilities;

namespace Game.Data;

public partial class EnemyRegistry : EntityRegistry
{
    protected override string ResourcePath => Constants.ENEMIES_PATH;

    public static Enemy PickRandom()
    {
        var enemies = Resources.Values.ToArray();
        var randomIndex = MathUtil.RNG.RandiRange(0, enemies.Length - 1);

        var enemyScene = enemies[randomIndex];
        return enemyScene.InstantiateOrNull<Enemy>();
    }

    public static Enemy PickRandomBoss()
    {
        var enemies = Resources.Values.Where(x => x.ResourcePath.Contains("Boss")).ToArray();
        var randomIndex = MathUtil.RNG.RandiRange(0, enemies.Length - 1);

        return enemies[randomIndex].InstantiateOrNull<Enemy>();
    }
}