using System;
using System.Diagnostics;
using System.Linq;
using Game.Common;
using Game.UI.Screens;
using Godot;
using GodotUtilities;

namespace Game.Autoload;

[Scene]
public partial class SceneManager : Autoload<SceneManager>
{
    private const float MAX_LOAD_TIME = 5.0f;

    [Node] private ResourcePreloader resourcePreloader;
    [Node] private Timer timer;

    private bool loading;
    private string loadPath;
    private Node from;
    private Node to;
    private Loading loadingScreen;
    private Loading.Transition transition = Loading.Transition.Fade;
    private readonly Stopwatch stopwatch = new();


    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        timer.Timeout += UpdateLoadStatus;
        from = GetTree().CurrentScene;
        to = GetTree().Root;
    }

    public static void ChangeScene(
        string path,
        Node from = null,
        Node to = null,
        Loading.Transition? transition = null
    )
    {
        if (!ResourceLoader.Exists(path))
        {
            Log.Error($"Scene '{path}' not found.");
            return;
        }

        var instance = Instance;

        if (instance.loading)
        {
            Log.Warn("Scene change already in progress.");
            return;
        }

        instance.Reset();
        instance.loading = true;
        instance.from = from ?? instance.GetTree().CurrentScene;
        instance.to = to ?? instance.GetTree().Root;
        instance.transition = transition ?? Loading.Transition.Fade;

        instance.LoadScene(path);
    }

    private async void LoadScene(string path)
    {
        stopwatch.Restart();

        loadingScreen = resourcePreloader.InstanceSceneOrNull<Loading>();
        GetTree().Root.AddChild(loadingScreen);
        await loadingScreen.Start(transition);

        if (!ResourceLoader.Exists(path))
        {
            Log.Error($"[LoadScene] Resource not found: {path}");
            Reset();
            return;
        }
        
        if (ResourceLoader.Load<PackedScene>(path) == null)
        {
            Log.Error($"[LoadScene] Failed to load scene: {path}");
            Reset();
            return;
        }
        
        if (ResourceLoader.LoadThreadedRequest(path, useSubThreads: true) != Error.Ok)
        {
            Log.Error($"[LoadScene] Threaded loading failed: {path}");
            Reset();
            return;
        }

        loadPath = path;
        timer.Start();
    }

    private void UpdateLoadStatus()
    {
        if (stopwatch.Elapsed.TotalSeconds > MAX_LOAD_TIME)
        {
            Log.Warn($"Loading scene '{loadPath}' took too long. Resetting.");
            var path = loadPath;
            ChangeScene(path, from, to, transition);
            return;
        }

        var progress = new Godot.Collections.Array();
        var status = ResourceLoader.LoadThreadedGetStatus(loadPath, progress);

        switch (status)
        {
            case ResourceLoader.ThreadLoadStatus.InvalidResource:
                Log.Error($"Invalid resource '{loadPath}'.");
                break;
            case ResourceLoader.ThreadLoadStatus.InProgress:
                if (progress.Count > 0)
                {
                    var p = progress[0].As<float>();
                    loadingScreen?.SetProgress(p);
                    Log.Debug($"Loading scene '{loadPath}': {p:P0}");
                }

                break;
            case ResourceLoader.ThreadLoadStatus.Failed:
                Log.Error($"Failed to load scene '{loadPath}'.");
                timer.Stop();
                loadingScreen?.SetProgress(1.0f);
                loadingScreen?.QueueFree();
                loadingScreen = null;
                loading = false;
                loadPath = null;
                break;
            case ResourceLoader.ThreadLoadStatus.Loaded:
                if (ResourceLoader.LoadThreadedGet(loadPath) is not PackedScene scene)
                {
                    Log.Error($"Failed to load scene '{loadPath}'.");
                    break;
                }

                FinishLoad(scene);

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async void FinishLoad(PackedScene scene)
    {
        var outgoing = from;
        var incoming = scene.InstantiateOrNull<Node>();

        if (incoming == null)
        {
            Log.Error($"Failed to instantiate scene '{loadPath}'.");
            Reset();
            return;
        }

        if (outgoing != GetTree().Root && IsInstanceValid(outgoing))
        {
            outgoing.QueueFree();
        }

        await loadingScreen.End();

        loadingScreen = null;

        to.AddChild(incoming);
        Reset();
    }

    private void Reset()
    {
        loading = false;
        loadPath = null;

        from = GetTree().CurrentScene;
        to = GetTree().Root;

        timer.Stop();

        loadingScreen?.QueueFree();
        loadingScreen = null;

        transition = Loading.Transition.Fade;
    }
}