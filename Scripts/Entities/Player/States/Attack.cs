using Game.Components;
using Game.Logic.States;
using Godot;
using GodotUtilities;

namespace Game.Entities.Player.States;

[Scene]
[Tool]
public partial class Attack : State
{
    [Node]
    private AnimatedSprite2D sprites;

    [Node]
    private StateMachine stateMachine;

    [Node]
    private StatsManager statsManager;

    private Player player;

    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;
        
        WireNodes();
        player = Owner as Player;
    }

    public override void Enter()
    {
        player.CanMove = false;
    }

    public override async void Update(double delta)
    {
        sprites.Play($"attack_{player.MoveDirection}");
        
        await ToSignal(sprites, "animation_finished");
        
        stateMachine.ChangeState("idle");
    }

    public override void PhysicsUpdate(double delta) { }

    public override void Exit()
    {
        player.CanMove = true;
    }
}