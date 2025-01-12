using System.Linq;
using Game.Globals;
using Game.UI.Overlays;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.Components.Managers;

[Scene]
public partial class GameManager : Node
{
    [Export] private bool showStartScreen;

    [Node] private CanvasLayer userInterface;
    [Node] private Node currentScene;
    [Node] private ResourcePreloader resourcePreloader;

    private Overlay currentOverlay;
    private Overlay GetOverlay(string name) => userInterface.GetNode<Overlay>(name);
    private static GameManager instance;

    public static Node CurrentScene => instance.currentScene;

    public static Overlay CurrentOverlay
    {
        get => instance.currentOverlay;
        set => instance.currentOverlay = value;
    }


    public override void _Ready()
    {
        instance = this;
        if (!showStartScreen && OS.IsDebugBuild()) return;

        currentScene.GetChildren().ToList().ForEach(c => c.QueueFree());
        var startScreen = resourcePreloader.GetResource<PackedScene>("StartScreen").Instantiate();
        currentScene.AddChild(startScreen);
    }

    public override void _Process(double delta)
    {
        // if (Input.MouseMode != Input.MouseModeEnum.ConfinedHidden)
        //     Input.SetMouseMode(Input.MouseModeEnum.ConfinedHidden);
        //
        // if (currentOverlay == null) return;
        //
        // if (currentOverlay.IsVisible())
        //     Input.SetMouseMode(Input.MouseModeEnum.Confined);
        // else
        //     currentOverlay = null;
    }

    private void HandleOverlay(string overlayName)
    {
        var targetOverlay = GetOverlay(overlayName);

        if (targetOverlay == null) return;

        var shouldClose = currentOverlay == targetOverlay || (overlayName == "Menu" && currentOverlay != null);

        if (shouldClose)
        {
            currentOverlay.Close();
            return;
        }

        if (currentOverlay != null) return;

        currentOverlay = targetOverlay;
        currentOverlay.Open();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("open_inventory"))
            HandleOverlay("Inventory");
        else if (@event.IsActionPressed("menu"))
            HandleOverlay("Menu");
        else if (@event.IsActionPressed("open_quests_info"))
            HandleOverlay("Quest");
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
            // TODO: save game
            return;

        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public static void OpenOverlay(string overlayName) => instance.HandleOverlay(overlayName);

    public static void CloseCurrentOverlay()
    {
        instance.currentOverlay?.Close();
        instance.currentOverlay = null;
    }

    public static void ChangeScene(
        string path,
        Vector2 direction = default,
        Transition transition = Transition.Fade
    )
    {
        SceneManager.ChangeScene(
            path,
            transition: transition,
            from: instance.currentScene.GetChildren().FirstOrDefault(),
            loadInTo: instance.currentScene,
            moveDirection: direction
        );
    }
}