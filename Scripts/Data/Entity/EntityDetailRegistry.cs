using System.Collections.Generic;
using Game.Common;
using Game.Common.Abstract;

namespace Game.Data;


public partial class EntityDetailRegistry : Registry<EntityDetail, EntityDetailRegistry>
{
    protected override string ResourcePath => Constants.ENTITY_DETAILS_PATH;

    public static IEnumerable<EntityDetail> Get(EntityDetail.EntityType type)
    {
        foreach (var detail in Resources.Values)
        {
            if (detail.Type != type) continue;

            yield return detail;
        }
    }
}
