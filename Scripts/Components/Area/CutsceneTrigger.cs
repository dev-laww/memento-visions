using System.Collections.Generic;
using Godot;
using DialogueManagerRuntime;
using Game.Utils.Extensions;

namespace Game.Components;

[Tool]
[GlobalClass]
public partial class CutsceneTrigger : Area2D
{
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
        DialogueManager.DialogueEnded += _ => this.GetPlayer()?.SetProcessInput(true);
    }

    private void OnBodyEntered(Node body)
    {
        if (triggered) return;

        var player = this.GetPlayer();
        player?.SetProcessInput(false);
        DialogueManager.ShowDialogueBalloon(Dialog, "Start");

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