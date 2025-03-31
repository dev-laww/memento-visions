using Godot;
using Godot.Collections;

namespace Game.Entities;

public abstract partial class Enemy : Entity
{
    public enum EnemyType
    {
        Common,
        Boss
    }

    public string BossName { get; private set; }
    public EnemyType Type { get; private set; }
    public override string ToString() => $"<Enemy ({Id})>";

    public override Array<Dictionary> _GetPropertyList()
    {
        var propertyList = new Array<Dictionary>();

        if (Type == EnemyType.Boss)
        {
            propertyList.Add(new()
            {
                { "name", PropertyName.BossName },
                { "type", (int)Variant.Type.String },
                { "usage", (int)PropertyUsageFlags.Default }
            });
        }

        propertyList.Add(new()
        {
            { "name", PropertyName.Type },
            { "type", (int)Variant.Type.Int },
            { "usage", (int)PropertyUsageFlags.Default },
            { "hint", (int)PropertyHint.Enum },
            { "hint_string", "Common,Boss" }
        });

        return propertyList;
    }

}