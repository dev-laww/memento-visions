using System;
using Game.UI.Screens;
using Godot;
using Array = Godot.Collections.Array;

namespace Game.AutoLoad;

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
            _ => "no_to_transition"
        };
    }
}

// TODO: Add wipe direction to Transition.Wipe
public partial class SceneManager : Global<SceneManager>
{
    [Signal] public delegate void LoadStartEventHandler(Loading loadingScreen);
    [Signal] public delegate void SceneAddedEventHandler(Node node, Loading loadingScreen);
    [Signal] public delegate void LoadCompleteEventHandler(Node node);
    [Signal] public delegate void ContentFinishedLoadingEventHandler(Node2D content);
    [Signal] public delegate void ContentInvalidEventHandler(string path);
    [Signal] public delegate void ContentFailedToLoadEventHandler(string path);

    private PackedScene loadingScreenScene =
        ResourceLoader.Load("res://Scenes/UI/Screens/Loading.tscn") as PackedScene;

    // TODO: cleanup
    private int WINDOW_WIDTH => GetViewport().GetWindow().Size.X;
    private int WINDOW_HEIGHT => GetViewport().GetWindow().Size.Y;
    private Loading loadingScreen;
    private Transition transition;
    private Vector2 direction;
    private string contentPath;
    private Timer loadProgressTimer;
    private Node loadSceneInTo;
    private Node sceneToUnload;
    private bool loading;

    public override void _Ready()
    {
        ContentInvalid += path => GD.PushError($"Content at path {path} is invalid.");
        ContentFailedToLoad += path => GD.PushError($"Failed to load content at path {path}.");
        ContentFinishedLoading += OnFinishedLoading;
    }

    public static async void ChangeScene(
        string to,
        Node from = null,
        Transition transition = Transition.Fade,
        Node loadInTo = null,
        Vector2 moveDirection = default
    )
    {
        if (Instance.loading)
        {
            GD.PushWarning("SceneManager is already loading a scene.");
            return;
        }

        if (moveDirection == default && transition == Transition.Zelda)
        {
            GD.PushWarning("Move direction is not set");
            return;
        }

        if (transition != Transition.Zelda && Instance.loadingScreen != null)
            await Instance.ToSignal(Instance.loadingScreen.animationPlayer, "animation_finished");

        Instance.loading = true;
        Instance.loadSceneInTo = loadInTo ?? Instance.GetTree().Root;
        Instance.sceneToUnload = from ?? Instance.GetTree().CurrentScene;
        Instance.transition = transition;
        Instance.direction = moveDirection;

        if (Instance.transition != Transition.Zelda)
        {
            Instance.loadingScreen = Instance.loadingScreenScene.Instantiate<Loading>();
            Instance.GetTree().Root.AddChild(Instance.loadingScreen);
            Instance.loadingScreen.StartTransition(Instance.transition);
        }

        Instance.LoadContent(to);
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
            loadingScreen = null;
        }

        EmitSignal(SignalName.LoadComplete, content);
        loading = false;
    }
}