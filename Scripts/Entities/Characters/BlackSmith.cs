using Game.Components.Area;
using Game.Components.Managers;
using Godot;
using GodotUtilities;

namespace Game.Entities.Characters;

[Scene]
public partial class BlackSmith : Entity
{
    [Node] private Interaction interaction;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void OnReady()
    {
        interaction.Interacted += OnInteracted;
    }

    private static void OnInteracted()
    {
        GameManager.OpenOverlay("Crafting");
    }
}