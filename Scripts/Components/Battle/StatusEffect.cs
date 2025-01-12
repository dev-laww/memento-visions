using Game.Components.Managers;
using Game.Entities;
using Godot;

namespace Game.Components.Battle;

public abstract partial class StatusEffect : Node
{
    [Export] public float Duration { get; private set; }

    public StatsManager Target { get; private set; }
    public float RemainingTime { get; private set; }
    public bool IsActive => RemainingTime > 0;

    public virtual void Apply(StatsManager target)
    {
        RemainingTime = Duration;
        Target = target;
    }

    public abstract void Remove();

    public virtual void Update(double delta)
    {
        if (!IsActive) return;

        RemainingTime -= (float)delta;
        OnTick(delta);
    }

    public abstract void OnTick(double delta);
}