using Godot;
using System;
using Game.Components;
using GodotUtilities;

namespace Game.World.Puzzle;

[Scene]
public partial class StreetLight : StaticBody2D
{
    [Node] private Interaction Interaction;
    [Node] private PointLight2D PostLight;
    private bool isLit = false;
    
    public override void _Ready()
    {
        base._Ready();
        Interaction.Interacted += OnInteract;
    }
    
    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated || Engine.IsEditorHint()) return;

        WireNodes();
    }

    private void OnInteract()
    {
        LightUp(!isLit);
        EmitSignalTorchLit();
    }
    
    public void LightUp(bool lit)
    {
        isLit = lit;
        PostLight.Enabled = lit;
    }
    
    public void ResetLight()
    {
        isLit = false;
        PostLight.Enabled = false;
    }
    
    [Signal]
    public delegate void TorchLitEventHandler();
}