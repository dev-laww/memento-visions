using System;
using System.Collections.Generic;
using Game.Globals;
using Godot;
using GodotUtilities;

namespace Game;

[Scene]
public partial class DeveloperConsole : Control
{
    [Node] LineEdit commandInput;
    [Node] CodeEdit output;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        CommandInterpreter.Register("quit", Quit, "Quits the game.");
        CommandInterpreter.Register("hi", Hi, "Prints 'Hello, World!'");

        commandInput.TextSubmitted += OnCommandInputSubmit;
        CommandInterpreter.CommandExecuted += OnCommandExecuted;
        output.Text = string.Empty;
    }

    private void OnCommandInputSubmit(string text)
    {
        CommandInterpreter.Execute(text);
        commandInput.Clear();
    }

    private void OnCommandExecuted(string command, object[] _)
    {
        var history = CommandInterpreter.History;

        var lines = new List<string>();

        foreach (var (Command, Timestamp, Exception) in history)
        {
            lines.Add($"{Timestamp:HH:mm} > {Command}");

            if (Exception != null && Exception is not InvalidOperationException or FormatException)
            {
                lines.Add($"{Exception}");
                lines.Add("\t" + string.Join("\n\t", Exception.StackTrace.Split('\n')));
            }
            else if (Exception != null)
            {
                lines.Add($"{Exception.Message}");
            }

        }

        output.Text = string.Join("\n", lines);
    }

    private void Quit()
    {
        GetTree().Quit();
    }

    private void Hi() => GD.Print("Hello, World!");
}

