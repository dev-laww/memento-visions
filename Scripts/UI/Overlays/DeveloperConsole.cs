using System;
using System.Collections.Generic;
using Game.Exceptions.Command;
using Game.Globals;
using Game.UI.Overlays;
using Godot;
using GodotUtilities;

namespace Game;

[Scene]
public partial class DeveloperConsole : Overlay
{
    [Node] LineEdit commandInput;
    [Node] CodeEdit output;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _EnterTree()
    {
        CommandInterpreter.Register("quit", Quit, "Quits the game.");
        CommandInterpreter.Register("clear", ClearOutput, "Clears the console output.");
        CommandInterpreter.Register("help", Help, "Displays all available commands.");
    }

    public override void _ExitTree()
    {
        CommandInterpreter.Unregister("quit");
        CommandInterpreter.Unregister("clear");
        CommandInterpreter.Unregister("help");
    }

    public override void _Ready()
    {
        commandInput.TextSubmitted += OnCommandInputSubmit;
        CommandInterpreter.CommandExecuted += OnCommandExecuted;
        output.Text = string.Empty;
    }

    private void OnCommandInputSubmit(string text)
    {
        CommandInterpreter.Execute(text);
        commandInput.Clear();
    }

    // TODO: Implement more gracefull printing
    private void OnCommandExecuted(string _command, object[] _args)
    {
        var history = CommandInterpreter.History;

        var lines = new List<string>();

        foreach (var (command, Timestamp, Exception) in history)
        {
            lines.Add($"{Timestamp:HH:mm} > {command}");

            if (Exception != null && Exception is not CommandException)
            {
                lines.Add($"{Exception}");
                lines.Add("\t" + string.Join("\n\t", Exception.StackTrace.Split('\n')));
            }
            else if (Exception != null)
            {
                lines.Add($"{Exception.Message}");
            }

            if (command == "help" || command == "?")
            {
                lines.Add("Available commands:");
                foreach (var (name, (_, description)) in CommandInterpreter.Commands)
                {
                    lines.Add($"\t{name} - {description}");
                }
            }
        }

        output.Text = string.Join("\n", lines);
    }

    private void Quit() => GetTree().Quit();

    private void ClearOutput()
    {
        output.Text = string.Empty;
        CommandInterpreter.ClearHistory();
    }

    private void Help() { }
}

