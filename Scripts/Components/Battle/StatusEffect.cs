using Game.Common;
using Game.Entities;
using Godot;

namespace Game.Components.Battle;

[GlobalClass]
public partial class StatusEffect : Node
{
    [Export] public string Id;
    [Export] public string StatusEffectName;
    [Export] public float Duration { get; private set; }
    [Export(PropertyHint.MultilineText)] public string Description;

#nullable enable
    protected Entity? Target => GetParent() as Entity;
#nullable disable

    public override void _Ready()
    {
        if (Target != null) return;

        Log.Error($"{this} must be a child of an Entity. Disabling...");
        QueueFree();
    }

    public void Update()
    {
        Duration -= (float)GetProcessDeltaTime();
        Tick();
    }

    protected virtual void Tick() { }

    public virtual void Apply() { }

    public virtual void Remove() { }


    public override string ToString() => $"<StatusEffect ({Id} on {Target?.Id})>";
}