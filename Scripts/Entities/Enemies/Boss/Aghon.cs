using Game.Components;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.Entities;

[Scene]
public partial class Aghon : Enemy
{
    private const string TRANSFORM = "transform";
    private const string MOVE = "move";
    private const string COMMON_ATTACK = "common_attack";
    private const string SPECIAL_ATTACK_1 = "special_attack_1";
    private const string SPECIAL_ATTACK_2 = "special_attack_2";

    [Node] private AnimationTree animationTree;
    [Node] private VelocityManager velocityManager;
    [Node] private PathFindManager pathFindManager;


    private AnimationNodeStateMachinePlayback playback;
    private int phase = 1;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void OnReady()
    {
        playback = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");

        StateMachine.AddStates(Normal, EnterNormal, ExitNormal);
        StateMachine.AddStates(CommonAttack);
        StateMachine.AddStates(SpecialAttack1);
        StateMachine.AddStates(SpecialAttack2);
        StateMachine.AddStates(TransformToSecondPhase);

        StateMachine.SetInitialState(Normal);
    }

    public override void OnProcess(double delta)
    {
        velocityManager.ApplyMovement();
        UpdateBlendPositions();
    }

    protected override void Die(DeathInfo info)
    {
        phase++;

        if (phase > 2)
        {
            base.Die(info);
            return;
        }

        StatsManager.Heal(100, StatsManager.ModifyMode.Percentage); // TODO: change to transform state instead of heal
    }

    #region States
    private void Normal()
    {
        pathFindManager.SetTargetPosition(this.GetPlayer()?.GlobalPosition ?? GlobalPosition);
        pathFindManager.Follow();
    }

    private void EnterNormal()
    {

    }

    private void ExitNormal()
    {

    }

    private void CommonAttack()
    {

    }

    private void SpecialAttack1()
    {

    }

    private void SpecialAttack2()
    {

    }

    private void TransformToSecondPhase()
    {

    }
    #endregion

    #region Utility
    private void UpdateBlendPositions()
    {
        animationTree.Set("parameters/move/blend_position", velocityManager.LastFacedDirection);
    }
    #endregion
}

