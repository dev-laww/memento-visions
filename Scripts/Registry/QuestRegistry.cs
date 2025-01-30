using Game.Common;
using Game.Resources;
using Godot;

namespace Game.Registry;

[GlobalClass]
public partial class QuestRegistry() : Registry<Quest, QuestRegistry>(Constants.QUESTS_PATH);