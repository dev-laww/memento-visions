using Game.Common;
using Game.Entities;
using Godot;

namespace Game.Components;

[GlobalClass]
public partial class StatusEffect : Node
{
    [Export] public string Id;
    [Export] public string StatusEffectName;
    [Export] public float Duration { get; private set; }
    [Export(PropertyHint.MultilineText)] public string Description;
    [Export] public bool EditorAdded { get; private set; }

    public int StackCount { get; protected set; } = 1;
    public float RemainingDuration;

#nullable enable
    protected Entity? Target => GetParent() as Entity;
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