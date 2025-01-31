using Godot;

namespace Game.Common.Interfaces;

public interface IInteractable
{
    Vector2 InteractionPosition { get; }
    void Interact();
    void ShowUI();
    void HideUI();
}