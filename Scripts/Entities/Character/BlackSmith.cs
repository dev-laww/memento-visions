using Game.Components.Area;
using Game.Components.Managers;
using Godot;
using GodotUtilities;

namespace Game;

[Scene]
public partial class BlackSmith : CharacterBody2D
{
    [Node] private Interaction interaction;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        interaction.Interacted += OnInteracted;
    }

    private static void OnInteracted()
    {
        GameManager.OpenOverlay("Crafting");
    }
}