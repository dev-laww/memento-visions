using System.Collections.Generic;
using Game.Resources;
using Godot;

namespace Game.Components.Area;

[Tool]
[GlobalClass]
public partial class QuestTrigger : Area2D
{
    private enum TriggerType
    {
        Give,
        Complete
    }

    [Export]
    public Quest Quest
    {
        get => _quest;
        set
        {
            _quest = value;
            NotifyPropertyListChanged();
        }
    }

    [Export] private TriggerType Type;
    [Export] private bool ShouldInteract;

    // TODO: implement quest interaction and triggering, objective completion

    private Quest _quest;

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (Quest == null)
            warnings.Add("Quest is not set.");

        return [..warnings];
    }
}