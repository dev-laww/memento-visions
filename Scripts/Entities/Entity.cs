using System.Collections.Generic;
using System.Linq;
using Game.Common;
using Game.Components;
using Game.Autoload;
using Game.Utils.Battle;
using Godot;
using GodotUtilities;
using GodotUtilities.Logic;
using Game.Utils.Extensions;

namespace Game.Entities;

/// <summary>
/// Abstract base class for all entities in the game.
/// Inherits from CharacterBody2D.
/// </summary>
[Scene]
public abstract partial class Entity : CharacterBody2D
{
    /// <summary>
    /// Contains information about the entity that spawned.
    /// </summary>
    public partial class SpawnInfo : GodotObject
    {
        public Entity Entity;
        public Vector2 Position;

        public SpawnInfo() { }

        public SpawnInfo(Entity entity)
        {
            Entity = entity;
            Position = entity.GlobalPosition;
        }

        public override string ToString() => $"<SpawnInfo ({Entity.Id} at {Position})>";
    }

    /// <summary>
    /// Contains information about the entity that died.
    /// </summary>
    public partial class DeathInfo : GodotObject
    {
        public Entity Victim;
        public Entity Killer;
        public Vector2 Position;

        public override string ToString() => $"<DeathInfo ({Victim.Id} killed by {Killer.Id} at {Position})>";
    }

    /// <summary>
    /// Unique name of the entity.
    /// </summary>
    [Export] public string Id;

    /// <summary>
    /// Determines if the entity is an NPC.
    /// </summary>
    [Export] private bool isNpc;

    /// <summary>
    /// Reference to the StatsManager node.
    /// </summary>
    [Node] public StatsManager StatsManager;

    /// <summary>
    /// Reference to the HurtBox node.
    /// </summary>
    [Node] private HurtBox HurtBox;

    /// <summary>
    /// Signal emitted when the entity dies.
    /// </summary>
    /// <param name="info">The death info.Contains the victim and the killer.</param>
    [Signal] public delegate void DeathEventHandler(DeathInfo info);

    /// <summary>
    /// State machine for managing entity states.
    /// </summary>
    protected DelegateStateMachine StateMachine { get; } = new();

    /// <summary>
    /// Handles the entity's death.
    /// Emits the Death signal and frees the entity.
    /// </summary>
    /// <param name="info">The killer entity.</param>
    protected virtual void Die(DeathInfo info)
    {
        QueueFree();
    }

    /// <summary>
    ///  Called when the node enters the scene tree for the first time.
    /// </summary>
    public virtual void OnReady() { }

    /// <summary>
    /// Handles input events.
    /// </summary>
    /// <param name="event">The input event.</param>
    public virtual void OnInput(InputEvent @event) { }

    /// <summary>
    /// Handles physics processing.
    /// </summary>
    /// <param name="delta">The time since the last physics update.</param>
    public virtual void OnPhysicsProcess(double delta) { }

    /// <summary>
    /// Handles processing.
    /// </summary>
    /// <param name="delta">The time since the last update.</param>
    public virtual void OnProcess(double delta) { }

    private DeathInfo deathInfo;
    private SpawnInfo spawnInfo;

    private void AttackReceived(Attack attack)
    {
        Log.Debug($"{this} received an attack from {attack.Source}.");

        var velocityManager = GetChildren().OfType<VelocityManager>().FirstOrDefault();

        if (attack.Knockback is not null && velocityManager is not null)
        {
            var sourceVelocityManager = attack.Source.GetChildren().OfType<VelocityManager>().FirstOrDefault();

            attack.Knockback.Direction = sourceVelocityManager.LastFacedDirection.DirectionTo(GlobalPosition).TryNormalize();

            var (direction, force) = attack.Knockback;
            velocityManager.Knockback(direction, force);
        }
    }

    private void StatDepleted(StatsType type)
    {
        if (type != StatsType.Health) return;

        deathInfo = deathInfo = new DeathInfo
        {
            Victim = this,
            Killer = StatsManager.LastReceivedAttack?.Source,
            Position = GlobalPosition
        };

        Log.Debug($"{this} died{(deathInfo.Killer != null ? $" from an attack by {deathInfo.Killer}" : "")}.");

        Die(deathInfo);
    }

    public sealed override void _Ready()
    {
        if (this is not Player)
        {
            CollisionMask = 1 << 0 | 1 << 1 | 1 << 2 | 1 << 3;
            NotifyPropertyListChanged();
        }

        if (Engine.IsEditorHint() || isNpc) return;

        TreeExiting += EmitDeath;
        StatsManager.AttackReceived += AttackReceived;
        StatsManager.StatDepleted += StatDepleted;
        spawnInfo = new SpawnInfo(this);

        GameEvents.EmitEntitySpawned(spawnInfo);

        if (this is Enemy)
            EnemyManager.Register(spawnInfo);

        OnReady();
    }

    public sealed override void _Input(InputEvent @event)
    {
        if (Engine.IsEditorHint()) return;

        OnInput(@event);
    }

    public override void _EnterTree()
    {
        if (IsConnected(SignalName.ChildEnteredTree, new Callable(this, "NotifyChange"))) return;

        Connect(SignalName.ChildEnteredTree, new Callable(this, "NotifyChange"));
        ChildExitingTree += NotifyChange;
        ChildOrderChanged += NotifyPropertyListChanged;
    }

    public sealed override void _PhysicsProcess(double delta)
    {
        if (Engine.IsEditorHint()) return;

        OnPhysicsProcess(delta);
    }

    public sealed override void _Process(double delta)
    {
        if (Engine.IsEditorHint()) return;

        StateMachine.Update();
        OnProcess(delta);
    }

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    private void EmitDeath()
    {
        if (deathInfo is null) return;

        if (this is Enemy)
            EnemyManager.Unregister(deathInfo);

        EmitSignalDeath(deathInfo);
        GameEvents.EmitEntityDied(deathInfo);
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        var statsManagers = GetChildren().OfType<StatsManager>().Count();

        if (statsManagers != 1 && !isNpc)
            warnings.Add($"Entity should have {(statsManagers == 0 ? "a" : "only one")} StatsManager node.");

        var hurtBoxes = GetChildren().OfType<HurtBox>().Count();

        if (hurtBoxes != 1 && !isNpc)
            warnings.Add($"Entity should have {(hurtBoxes == 0 ? "a" : "only one")} HurtBox node.");

        if (Id is null or "")
            warnings.Add("Entity should have a unique Id.");

        return [.. warnings];
    }

    private void NotifyChange(Node _) => NotifyPropertyListChanged();

    public override string ToString() => $"<Entity ({Id})>";
}