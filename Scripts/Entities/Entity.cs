using System;
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
    private string UniqueName;

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
    protected DelegateStateMachine StateMachine = new();

    /// <summary>
    /// Called when the node is added to the scene.
    /// Initializes the entity and checks for required nodes.
    /// </summary>
    public override void _Ready()
    {
        // string message = null;
        //
        // switch (HurtBox)
        // {
        //     case null when StatsManager == null:
        //         message = "Entity must have a HurtBox and StatsManager node";
        //         break;
        //     case null:
        //         message = "Entity must have a HurtBox node";
        //         break;
        //     default:
        //         if (StatsManager == null)
        //             message = "Entity must have a StatsManager node";
        //         break;
        // }
        //
        // if (message != null) throw new NullReferenceException(message);
        
        StatsManager.StatsDepleted += OnStatsDepleted;
    }

    /// <summary>
    /// Handles the entity's death.
    /// Emits the Death signal and frees the entity.
    /// </summary>
    /// <param name="entity">The entity that died.</param>
    protected virtual void Die(Entity entity)
    {
        EmitSignal(SignalName.Death, entity);
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
}