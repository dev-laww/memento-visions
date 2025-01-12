using Godot;
using Game.Components.Managers;
using Game.Entities.Player;


namespace Game.Components.Area;

[Tool]
[GlobalClass]
public partial class TransitionArea : Area2D
{
    [Export]
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

    public override void _Ready()
    {
        CollisionLayer = 0;
        CollisionMask = 1 << 2;
        BodyEntered += OnBodyEntered;
        NotifyPropertyListChanged();
    }

    private void OnBodyEntered(Node body)
    {
        if (body is not Player) return;

        GameManager.ChangeScene(targetScene);
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new System.Collections.Generic.List<string>();

        if (TargetScene == null)
            warnings.Add("TargetScene is not set.");

        return warnings.ToArray();
    }
}