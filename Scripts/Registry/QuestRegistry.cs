using System;
using Game.Resources;
using Game.Utils;
using Godot;

namespace Game.Registry;

[GlobalClass]
public partial class QuestRegistry() : Registry<Quest, QuestRegistry>(Constants.QUESTS_PATH);