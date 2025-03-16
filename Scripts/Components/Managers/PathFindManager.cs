using System.Collections.Generic;
using Game.Components;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game;

[Scene]
[Tool, Icon("res://assets/icons/pathfind-manager.svg")]
public partial class PathFindManager : Node2D
{
    [Export]
    private VelocityManager VelocityManager
    {
        get => velocityManager;
        set
        {
            velocityManager = value;
            UpdateConfigurationWarnings();
        }
    }

    [Export] private int maxChangeDirectionDegrees = 0;

    [Export]
    private bool DebugEnabled
    {
        get => GetNode<NavigationAgent2D>("NavigationAgent2D").DebugEnabled;
        set
        {
            var agent = GetNode<NavigationAgent2D>("NavigationAgent2D");
            agent.DebugEnabled = value;
            agent.NotifyPropertyListChanged();
        }
    }

    [Node] public NavigationAgent2D NavigationAgent2D;
    [Node] private Timer intervalTimer;

    private VelocityManager velocityManager;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        NavigationAgent2D.DebugEnabled = DebugEnabled;
        NavigationAgent2D.VelocityComputed += OnVelocityComputed;
    }

    public void SetTargetPosition(Vector2 position)
    {
        if (!intervalTimer.IsStopped()) return;
        ForceSetTargetPosition(position);
    }

    public void Follow()
    {
        if (NavigationAgent2D.IsNavigationFinished())
        {
            VelocityManager.Decelerate();
            return;
        }

        var direction = (NavigationAgent2D.GetNextPathPosition() - GlobalPosition).TryNormalize();

        if (maxChangeDirectionDegrees > 0)
        {
            var radians = direction.Angle();
            var currentRadians = VelocityManager.Velocity.Angle();
            var maxRadians = Mathf.DegToRad(maxChangeDirectionDegrees);

            var clamped = Mathf.Clamp(radians, currentRadians - maxRadians, currentRadians + maxRadians);

            direction = Vector2.Right.Rotated(clamped);
        }

        VelocityManager.Accelerate(direction);
    }

    public Vector2 GetTargetPosition() => NavigationAgent2D.GetNextPathPosition();

    public void ForceSetTargetPosition(Vector2 position)
    {
        NavigationAgent2D.TargetPosition = position;
        intervalTimer.Call("start_random");
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (VelocityManager == null)
            warnings.Add("VelocityManager is not set.");

        return [.. warnings];
    }

    private void OnVelocityComputed(Vector2 velocity)
    {
        if (Engine.IsEditorHint()) return;

        VelocityManager.AccelerateToVelocity(velocity);
    }
}