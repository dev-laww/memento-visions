using System.Linq;
using Game.Components;
using Game.Entities;
using Godot;
using GodotUtilities;

namespace Game.Data;

[GlobalClass, Icon("res://assets/icons/status-effect.svg")]
public partial class StatusEffect : Resource
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
    [Export] public int MaxStacks = 1;

    public int StackCount { get; protected set; } = 1;
    public float RemainingDuration;

    protected Entity Target;
    protected VelocityManager TargetVelocityManager => Target.GetChildrenOfType<VelocityManager>().FirstOrDefault();
    protected StatsManager TargetStatsManager => Target.StatsManager;

    public void Update()
    {
        RemainingDuration -= (float)Target.GetPhysicsProcessDeltaTime();
        Tick();
    }

    public void ApplyStatusEffect(Entity target)
    {
        Target = target;
        RemainingDuration += Duration;
        Apply();
    }

    protected virtual void Tick() { }

    public virtual void Apply() { }

    public virtual void Remove() { }

    public virtual void Stack(int amount = 1) { }


    public override string ToString() => $"<StatusEffect ({Id})>";
}