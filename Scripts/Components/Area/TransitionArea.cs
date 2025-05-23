using Godot;
using Game.Entities;


namespace Game.Components;

[Tool]
[GlobalClass]
public partial class TransitionArea : Area2D
{
    [Export(PropertyHint.File, "*.tscn")]
    public string TargetScene
    {
        get => targetScene;
        set
        {
            targetScene = value;
            NotifyPropertyListChanged();
        }
    }

    private string targetScene;
    private bool isEnabled = true;

    public override void _Ready()
    {
        CollisionLayer = 0;
        CollisionMask = 1 << 2;
        BodyEntered += OnBodyEntered;
        NotifyPropertyListChanged();
    }

    public void Toggle(bool enabled = true) => isEnabled = enabled;

    private void OnBodyEntered(Node body)
    {
        if (body is not Player || !isEnabled) return;

        GameManager.ChangeScene(targetScene);
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new System.Collections.Generic.List<string>();

        if (TargetScene == null)
            warnings.Add("TargetScene is not set.");

        return [.. warnings];
    }
}