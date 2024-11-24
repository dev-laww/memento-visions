using System.Collections.Generic;
using Godot;
using DialogueManagerRuntime;
using Game.Utils.Extensions;

namespace Game.Components.Area;

[Tool]
[GlobalClass]
public partial class CutsceneTrigger : Area2D
{
    // TODO: add resource hint for dialog
    [Export]
    private Resource Dialog
    {
        get => resource;
        set
        {
            resource = value;
            UpdateConfigurationWarnings();
        }
    }

    private Resource resource;
    private bool triggered;

    public override void _Ready()
    {
        CollisionMask = 1 << 2;
        CollisionLayer = 1 << 4;
        BodyEntered += OnBodyEntered;
        DialogueManager.DialogueEnded += dialogueResource => this.GetPlayer().SetProcessInput(true);
    }

    private void OnBodyEntered(Node body)
    {
        if (triggered) return;
        
        var player = this.GetPlayer();
        player.SetProcessInput(false);
        DialogueManager.ShowDialogueBalloon(Dialog, "START");
        
        triggered = true;
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();
        
        if (Dialog == null)
            warnings.Add("Dialog is not set.");
        
        return warnings.ToArray();
    }
}