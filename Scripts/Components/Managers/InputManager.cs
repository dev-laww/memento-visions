using System.Collections.Generic;
using Godot;

namespace Game.Components;

[GlobalClass, Icon("res://assets/icons/input-manager.svg")]
public partial class InputManager : Node
{
    // TODO: Make keybinds rebindable

    private readonly StringName moveLeft = "move_left";
    private readonly StringName moveRight = "move_right";
    private readonly StringName moveUp = "move_up";
    private readonly StringName moveDown = "move_down";
    private readonly StringName attack = "attack";
    private readonly StringName dash = "dash";


    private readonly HashSet<string> justPressed = [];
    private readonly HashSet<string> justReleased = [];
    private readonly HashSet<string> pressed = [];

    private Callable clearJustPressed;
    private int lockCount;

    public override void _Ready()
    {
        clearJustPressed = Callable.From(ClearJustPressed);
    }

    public override void _Process(double delta)
    {
        clearJustPressed.CallDeferred();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (lockCount > 0)
            return;

        Track(moveLeft, @event);
        Track(moveRight, @event);
        Track(moveUp, @event);
        Track(moveDown, @event);
        Track(attack, @event);
        Track(dash, @event);
    }

    private void Track(StringName name, InputEvent @event)
    {
        if (!@event.IsAction(name)) return;

        if (@event.IsActionPressed(name))
        {
            pressed.Add(name);
            justPressed.Add(name);
        }
        else if (@event.IsActionReleased(name) && pressed.Contains(name))
        {
            pressed.Remove(name);
            justReleased.Add(name);
        }
    }

    public Vector2 GetVector() => lockCount > 0 ? Vector2.Zero : Input.GetVector(moveLeft, moveRight, moveUp, moveDown);

    public bool IsActionPressed(StringName name) => pressed.Contains(name);

    public bool IsActionJustPressed(StringName name) => justPressed.Contains(name);

    public bool IsActionJustReleased(StringName name) => justReleased.Contains(name);

    public void AddLock()
    {
        lockCount++;

        GD.Print($"Input lock count: {lockCount}");

        if (lockCount == 1)
        {
            pressed.Clear();
            justPressed.Clear();
            justReleased.Clear();
        }
    }

    public void RemoveLock() => lockCount = Mathf.Max(0, lockCount - 1);

    public void ClearJustPressed()
    {
        justPressed.Clear();
        justReleased.Clear();
    }
}