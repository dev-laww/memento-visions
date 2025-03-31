using Game.Autoload;
using Game.Entities;
using Godot;

namespace Game.Data;

public partial class Electrocute : StatusEffect
{
    [Export] private float damagePerSecond = 2f;

    private float damageAccumulated;

    private readonly Color color = new(0.046f, 0.619f, 0.954f);

    private float accumulatedDamage;

    protected override void Tick()
    {
        accumulatedDamage += damagePerSecond * (float)Target.GetPhysicsProcessDeltaTime();

        if (accumulatedDamage < 1) return;

        TargetStatsManager.TakeDamage(accumulatedDamage);

        var text = FloatingTextManager.SpawnFloatingText(new FloatingTextManager.FloatingTextSpawnArgs
        {
            Color = color,
            Duration = 1f,
            Text = "Electrocuted",
            Position = Target.GlobalPosition + (Vector2.Up * 16),
            Parent = Target
        });
        text.Finished += text.QueueFree;

        (Target as Player)?.InputManager.AddLock();
        TargetVelocityManager.Decelerate(force: true);
        Target.SetPhysicsProcess(false);
        Target.SetProcess(false);

        Target.GetTree().CreateTimer(0.1f).Timeout += () =>
        {
            (Target as Player)?.InputManager.RemoveLock();
            Target.SetPhysicsProcess(true);
            Target.SetProcess(true);
        };

        accumulatedDamage = 0;
    }


    public override void Remove()
    {
        (Target as Player)?.InputManager.RemoveLock();
        Target.SetPhysicsProcess(true);
        Target.SetProcess(true);
    }
}
