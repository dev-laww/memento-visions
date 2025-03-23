using Game.Common;
using Game.Common.Abstract;

namespace Game.Data;

public partial class StatusEffectRegistry : Registry<StatusEffect, StatusEffectRegistry>
{
    protected override string ResourcePath => Constants.STATUS_EFFECTS_PATH;
}