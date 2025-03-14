using System.Security.Cryptography;
using Game.Components;
using Game.Entities;
using Game.UI.Screens;
using Godot;
using GodotUtilities;

namespace Game;

[Scene]
public partial class Aswang : Entity
{
    const string START_RANDOM = "start_random";
    const string SPECIAL_ATTACK = "Special Attack";
    const string COMMON_ATTACK = "Common Attack";
    const string MOVE = "Move";


    [Node] private AnimationTree animationTree;
    [Node] private Timer attackTimer;
    [Node] private VelocityManager velocityManager;
    [Node] private PathFindManager pathFindManager;

    private AnimationNodeStateMachinePlayback playback;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void OnReady()
    {
        playback = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");

        StateMachine.AddStates(Move, EnterMove);
        StateMachine.AddStates(CommonAttack, EnterCommonAttack);
        StateMachine.AddStates(SpecialAttack, EnterSpecialAttack);

        StateMachine.SetInitialState(Move);
    }

    public void EnterMove()
    {
        playback.Travel(MOVE);
    }

    private void Move()
    {

    }


    public void EnterCommonAttack()
    {
        playback.Travel(COMMON_ATTACK);
    }

    private void CommonAttack()
    {

    }


    public void EnterSpecialAttack()
    {
        playback.Travel(SPECIAL_ATTACK);
    }

    private void SpecialAttack()
    {

    }
}

