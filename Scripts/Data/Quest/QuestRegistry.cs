using Game.Common;
using Game.Common.Abstract;
using Godot;

namespace Game.Data;

[GlobalClass]
public partial class QuestRegistry : Registry<Quest, QuestRegistry>
{
    protected override string ResourcePath => Constants.QUESTS_PATH;
}