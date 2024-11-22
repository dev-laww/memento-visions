using System;
using Godot;
using GodotUtilities;
using Array = Godot.Collections.Array;

namespace Game.Globals;

public enum Transition
{
    Fade,
    FadeWhite,
    Wipe,
    Zelda,
    None
}

public static class TransitionExtensions
{
    public static string ToValue(this Transition transition)
    {
        return transition switch
        {
            Transition.Fade => "fade_to_black",
            Transition.FadeWhite => "fade_to_white",
            Transition.Wipe => "wipe_to_right",
            Transition.Zelda => "zelda",
            Transition.None => "no_to_transition",
            _ => "no_to_transition",
        };
    }
}

// TODO: Add wipe direction to Transition.Wipe
public partial class SceneManager : Node
{
    [Signal]
    public delegate void LoadStartEventHandler(LoadingScreen loadingScreen);

    [Signal]
    public delegate void SceneAddedEventHandler(Node node, LoadingScreen loadingScreen);

    [Signal]
    public delegate void LoadCompleteEventHandler(Node node);

    [Signal]
    public delegate void ContentFinishedLoadingEventHandler(Node2D content);

    [Signal]
    public delegate void ContentInvalidEventHandler(string path);

    [Signal]
    public delegate void ContentFailedToLoadEventHandler(string path);

    public static SceneManager Instance { get; private set; }

    private PackedScene loadingScreenScene =
        ResourceLoader.Load("res://Scenes/UI/Screens/LoadingScreen.tscn") as PackedScene;

    private const int WINDOW_WIDTH = 640;
    private const int WINDOW_HEIGHT = 360;
    private LoadingScreen loadingScreen;
    private Transition transition;
    private Vector2 direction;
    private string contentPath;
    private Timer loadProgressTimer;
    private Node loadSceneInTo;
    private Node sceneToUnload;
    private bool loading;

    public override void _Ready()
    {
        Instance = this;
        ContentInvalid += path => GD.PushError($"Content at path {path} is invalid.");
        ContentFailedToLoad += path => GD.PushError($"Failed to load content at path {path}.");
        ContentFinishedLoading += OnFinishedLoading;
    }

    public async void ChangeScene(
        string to,
        Node from = null,
        Transition transitionType = Transition.Fade,
        Node loadInTo = null,
        Vector2 moveDirection = default
    )
    {
        if (loading)
        {
            GD.PushWarning("SceneManager is already loading a scene.");
            return;
        }

        if (moveDirection == default && transitionType == Transition.Zelda)
        {
            GD.PushWarning("Move direction is not set");
            return;
        }

        if (transitionType != Transition.Zelda && loadingScreen != null)
            await ToSignal(loadingScreen.animationPlayer, "animation_finished");

        loading = true;
        loadSceneInTo = loadInTo ?? GetTree().Root;
        sceneToUnload = from ?? GetTree().CurrentScene;
        transition = transitionType;
        direction = moveDirection;

        if (transition != Transition.Zelda)
        {
            loadingScreen = loadingScreenScene.Instantiate<LoadingScreen>();
            GetTree().Root.AddChild(loadingScreen);
            loadingScreen.StartTransition(transition);
        }

        LoadContent(to);
    }

    private async void LoadContent(string path)
    {
        if (transition != Transition.Zelda)
            await ToSignal(loadingScreen.animationPlayer, "animation_finished");

        EmitSignal(SignalName.LoadStart, loadingScreen);
        contentPath = path;

        var loader = ResourceLoader.LoadThreadedRequest(path);

        if (!ResourceLoader.Exists(path) || loader != Error.Ok)
        {
            EmitSignal(SignalName.ContentInvalid, path);
            return;
        }

        loadProgressTimer = new Timer
        { WaitTime = 0.1f };
        loadProgressTimer.Timeout += MonitorLoadStatus;
        GetTree().Root.AddChild(loadProgressTimer);
        loadProgressTimer.Start();
    }

    private void MonitorLoadStatus()
    {
        Array loadProgress = new();
        var loadStatus = ResourceLoader.LoadThreadedGetStatus(contentPath, loadProgress);

        switch (loadStatus)
        {
            case ResourceLoader.ThreadLoadStatus.InvalidResource:
                EmitSignal(SignalName.ContentInvalid, contentPath);
                loadProgressTimer.Stop();
                break;
            case ResourceLoader.ThreadLoadStatus.InProgress:
                loadingScreen?.UpdateBar((float)loadProgress[0] * 100);
                break;
            case ResourceLoader.ThreadLoadStatus.Failed:
                EmitSignal(SignalName.ContentFailedToLoad, contentPath);
                loadProgressTimer.Stop();
                break;
            case ResourceLoader.ThreadLoadStatus.Loaded:
                loadProgressTimer.Stop();
                loadProgressTimer.QueueFree();
                EmitSignal(
                    SignalName.ContentFinishedLoading,
                    (ResourceLoader.LoadThreadedGet(contentPath) as PackedScene)?.Instantiate()
                );
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async void OnFinishedLoading(Node2D content)
    {
        var outGoingScene = sceneToUnload;
        loadSceneInTo.AddChild(content);
        EmitSignal(SignalName.SceneAdded, content, loadingScreen);

        if (transition == Transition.Zelda)
        {
            content.Position = content.Position with
            {
                X = direction.X * WINDOW_WIDTH,
                Y = direction.Y * WINDOW_HEIGHT
            };

            var tweenIn = CreateTween();
            tweenIn.TweenProperty(content, "position", Vector2.Zero, 1f).SetTrans(Tween.TransitionType.Sine);

            var resetVector = new Vector2
            {
                X = direction.X * WINDOW_WIDTH,
                Y = direction.Y * WINDOW_HEIGHT
            };
            var tweenOut = CreateTween();
            tweenOut.TweenProperty(outGoingScene, "position", resetVector, 1f).SetTrans(Tween.TransitionType.Sine);

            await ToSignal(tweenIn, "finished");
        }

        if (outGoingScene != GetTree().Root)
            outGoingScene?.QueueFree();

        if (loadingScreen != null && transition != Transition.Zelda)
        {
            loadingScreen.FinishTransition();
            await ToSignal(loadingScreen.animationPlayer, "animation_finished");
        }

        EmitSignal(SignalName.LoadComplete, content);
        loading = false;
    }
}