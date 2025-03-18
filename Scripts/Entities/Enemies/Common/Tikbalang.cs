using Game.Components;
using Godot;
using GodotUtilities;

namespace Game.Entities;

[Scene]
public partial class Tikbalang : Entity
{
    private const string START_RANDOM = "start_random";
    private const string SPECIAL_ATTACK = "Special Attack";
    private const string COMMON_ATTACK = "Common Attack";
    private const string IDLE = "Idle";
    private const string MOVE = "Move";

    [Node] private VelocityManager velocityManager;
    [Node] private PathFindManager pathFindManager;
    [Node] private AnimationTree animationTree;

    private AnimationNodeStateMachinePlayback playback;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void OnReady()
    {
        playback = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");

        StateMachine.AddStates(Normal);
        StateMachine.AddStates(Move);
        StateMachine.AddStates(CommonAttack);
        StateMachine.AddStates(SpecialAttack);

        StateMachine.SetInitialState(Normal);
    }

    public override void OnProcess(double delta)
    {
        velocityManager.ApplyMovement();
    }

    private void Normal()
    {

    }

    private void Move()
    {

    }

    private void SpecialAttack()
    {

    }

    private void CommonAttack()
    {

    }

    private void UpdateBlendPosition()
    {
        animationTree.Set("parameters/Move/blend_position", velocityManager.LastFacedDirection);
        animationTree.Set("parameters/Common Attack/blend_position", velocityManager.LastFacedDirection);
        animationTree.Set("parameters/Special Attack/blend_position", velocityManager.LastFacedDirection);
        animationTree.Set("parameters/Idle/blend_position", velocityManager.LastFacedDirection);
    }
}

