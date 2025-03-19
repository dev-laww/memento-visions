using System.Linq;
using Game.Common;
using Game.Entities;
using Godot;
using GodotUtilities;

namespace Game.Components;

[GlobalClass]
public partial class StatusEffect : Node
{
    public class Info
    {
        public string Id;
        public bool IsGuaranteed;
        public float Chance;
        public int Turns;
    }

    [Export] public string Id;
    [Export] public string StatusEffectName;
    [Export] public float Duration { get; private set; }
    [Export(PropertyHint.MultilineText)] public string Description;
    [Export] public bool EditorAdded { get; private set; }
    [Export] public int MaxStacks = 1;

    public int StackCount { get; protected set; } = 1;
    public float RemainingDuration;

#nullable enable
    protected Entity? Target => GetParent() as Entity;
    protected VelocityManager? TargetVelocityManager => Target?.GetChildrenOfType<VelocityManager>().FirstOrDefault();
    protected StatsManager? TargetStatsManager => Target?.GetChildrenOfType<StatsManager>().FirstOrDefault();
#nullable disable

    public override void _Ready()
    {
        RemainingDuration += Duration;

        if (Target != null) return;

        Log.Error($"{this} must be a child of an Entity. Disabling...");
        QueueFree();
    }

    public void Update()
    {
        RemainingDuration -= (float)GetPhysicsProcessDeltaTime();
        Tick();
    }

    protected virtual void Tick() { }

    public virtual void Apply() { }

    public virtual void Remove() { }

    public virtual void Stack(int amount = 1) { }


    public override string ToString() => $"<StatusEffect ({Id})>";
}