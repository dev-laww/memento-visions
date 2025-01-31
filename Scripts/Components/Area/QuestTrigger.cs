using System.Collections.Generic;
using Game.Common.Interfaces;
using Game.Resources;
using Godot;

namespace Game.Components.Area;

[Tool]
[GlobalClass]
public partial class QuestTrigger : Area2D, IInteractable
{
    private enum TriggerType
    {
        Start,
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

    private Quest _quest;

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (Quest == null)
            warnings.Add("Quest is not set.");

        return [..warnings];
    }
    
    public Vector2 InteractionPosition => GlobalPosition;
    public void Interact()
    {
        throw new System.NotImplementedException();
    }
    public void ShowUI()
    {
        throw new System.NotImplementedException();
    }
    public void HideUI()
    {
        throw new System.NotImplementedException();
    }
}