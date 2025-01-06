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
    private static GameManager instance;
    public static Node CurrentScene => instance.currentScene;
    private Overlay GetOverlay(string name) => userInterface.GetNode<Overlay>(name);

    public override void _Ready()
    {
        instance = this;
        if (!showStartScreen && OS.IsDebugBuild()) return;

        currentScene.GetChildren().ToList().ForEach(c => c.QueueFree());
        var startScreen = resourcePreloader.GetResource<PackedScene>("StartScreen").Instantiate();
        currentScene.AddChild(startScreen);

        Input.SetMouseMode(Input.MouseModeEnum.Visible);
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
        // TODO: Fix this causes bugs, you cant open an overlay sometimes

        if (overlayName == "Menu" && currentOverlay != null)
        {
            currentOverlay.Close();
            currentOverlay = null;
            return;
        }

        var targetOverlay = GetOverlay(overlayName);

        if (currentOverlay == targetOverlay)
        {
            currentOverlay?.Toggle();
            currentOverlay = null;
            return;
        }

        if (currentOverlay != null)
            return;

        currentOverlay = targetOverlay;
        currentOverlay?.Toggle();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("open_inventory"))
            HandleOverlay("Inventory");
        else if (@event.IsActionPressed("menu"))
            HandleOverlay("Menu");
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