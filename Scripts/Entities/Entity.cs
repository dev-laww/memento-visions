using Game.Components.Area;
using Game.Components.Managers;
using Godot;
using GodotUtilities;
using GodotUtilities.Logic;

namespace Game.Entities;

/// <summary>
/// Abstract base class for all entities in the game.
/// Inherits from CharacterBody2D.
/// </summary>
[Scene]
public abstract partial class Entity : CharacterBody2D
{
    /// <summary>
    /// Unique name of the entity.
    /// </summary>
    [Export]
    public string Id;

    /// <summary>
    /// Reference to the StatsManager node.
    /// </summary>
    [Node]
    protected StatsManager StatsManager;

    /// <summary>
    /// Reference to the HurtBox node.
    /// </summary>
    [Node]
    private HurtBox HurtBox;

    /// <summary>
    /// Signal emitted when the entity dies.
    /// </summary>
    /// <param name="entity">The entity that died.</param>
    [Signal]
    public delegate void DeathEventHandler(Entity entity);

    /// <summary>
    /// State machine for managing entity states.
    /// </summary>
    protected DelegateStateMachine StateMachine;

    /// <summary>
    /// Called when the node is added to the scene.
    /// Initializes the entity and checks for required nodes.
    /// </summary>
    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;

        StateMachine = new();
        StatsManager.StatDepleted += OnStatsDepleted;
        OnReady();
    }

    /// <summary>
    /// Handles the entity's death.
    /// Emits the Death signal and frees the entity.
    /// </summary>
    /// <param name="killer">The killer entity.</param>
    // TODO: Add killer parameter
    protected virtual void Die(Entity killer)
    {
        EmitSignal(SignalName.Death, this);
        QueueFree();
    }


    /// <summary>
    /// Called when the entity's stats are depleted.
    /// If health is depleted, the entity dies.
    /// </summary>
    /// <param name="type">The type of stat that was depleted.</param>
    protected virtual void OnStatsDepleted(StatsType type)
    {
        if (type != StatsType.Health) return;

        Die(this);
    }

    /// <summary>
    ///  Called when the node enters the scene tree for the first time.
    /// </summary>
    protected virtual void OnReady() { }

    /// <summary>
    /// Handles input events.
    /// </summary>
    /// <param name="event">The input event.</param>
    protected virtual void OnInput(InputEvent @event) { }

    /// <summary>
    /// Handles physics processing.
    /// </summary>
    /// <param name="delta">The time since the last physics update.</param>
    protected virtual void OnPhysicsProcess(double delta) { }

    /// <summary>
    /// Handles processing.
    /// </summary>
    /// <param name="delta">The time since the last update.</param>
    protected virtual void OnProcess(double delta) { }

    public override void _Input(InputEvent @event)
    {
        if (Engine.IsEditorHint()) return;

        OnInput(@event);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Engine.IsEditorHint()) return;

        StateMachine.Update();
        OnPhysicsProcess(delta);
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint()) return;

        OnProcess(delta);
    }
}