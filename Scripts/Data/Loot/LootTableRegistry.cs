using System.Linq;
using Game.Common;
using Game.Common.Abstract;
using GodotUtilities;

namespace Game.Data;

public partial class LootTableRegistry : Registry<LootTable, LootTableRegistry>
{
    protected override string ResourcePath => Constants.LOOT_TABLES_PATH;

    public static LootTable GetRandom()
    {
        var lootTables = Resources.Values.ToArray();

        if (lootTables.Length == 0) return null;

        var randomIndex = MathUtil.RNG.RandiRange(0, lootTables.Length - 1);

        return lootTables[randomIndex].Duplicate() as LootTable;
    }
}
