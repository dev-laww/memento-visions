using System.Collections.Generic;
using System.Linq;
using Game.Common;
using Game.Common.Utilities;
using Game.Exceptions;
using Game.UI.Overlays;
using Godot;
using GodotUtilities;

namespace Game;

// TODO: fix console problem
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

        commandInput.TextSubmitted += OnCommandInputSubmit;
        CommandInterpreter.CommandExecuted += OnCommandExecuted;
    }

    public override void _ExitTree()
    {
        CommandInterpreter.Unregister("quit");
        CommandInterpreter.Unregister("clear");
        CommandInterpreter.Unregister("help");

        commandInput.TextSubmitted -= OnCommandInputSubmit;
        CommandInterpreter.CommandExecuted -= OnCommandExecuted;
    }

    public override void _Ready()
    {
        if (!OS.IsDebugBuild())
        {
            QueueFree();
            return;
        }

        output.Text = string.Empty;
        commandInput.GrabFocus();
        OnCommandExecuted(string.Empty, []);
    }

    private void OnCommandInputSubmit(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        CommandInterpreter.Execute(text);
        commandInput.Clear();
        commandInput.GrabFocus();
    }

    // TODO: Implement more gracefull printing
    private void OnCommandExecuted(string _command, object[] _args)
    {
        var history = CommandInterpreter.History;

        var lines = new List<string>();

        foreach (var (command, Timestamp, Exception) in history.Reverse())
        {
            lines.Add($"{Timestamp:HH:mm} > {command}");

            if (Exception != null && Exception is not CommandException)
            {
                lines.Add($"{Exception}");

                if (Exception.StackTrace != null)
                    lines.Add("\t" + string.Join("\n\t", Exception.StackTrace.Split('\n')));
            }
            else if (Exception != null)
            {
                lines.Add($"{Exception.Message}");
            }

            if (command != "help" && command != "?") continue;

            lines.Add("Available commands:");
            foreach (var (name, (_, description)) in CommandInterpreter.Commands)
            {
                lines.Add($"\t{name} - {description}");
            }
        }

        output.Text = string.Join("\n", lines);
    }

    private void Quit() => GetTree().Quit();

    private void ClearOutput()
    {
        output.Text = string.Empty;
        CommandInterpreter.ClearHistory();
        Log.Debug("Console history cleared.");
    }

    private static void Help() { }

    public override void Close()
    {
        base.Close();

        commandInput.Text = string.Empty;
    }
}