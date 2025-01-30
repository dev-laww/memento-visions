using Godot;

namespace Game.Common.Interfaces;

public interface IEntity
{
    void OnReady();
    void OnProcess(double delta);
    void OnPhysicsProcess(double delta);
    void OnInput(InputEvent @event);
}