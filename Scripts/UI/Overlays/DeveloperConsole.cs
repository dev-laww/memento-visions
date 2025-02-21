using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.Linq;
using Game.Common;
using Game.Common.Utilities;
using Game.UI.Overlays;
using Godot;
using GodotUtilities;

namespace Game;

[Scene]
public partial class DeveloperConsole : Overlay
{
    [Node] LineEdit commandInput;
    [Node] RichTextLabel output;

    public static InterpreterConsole Console { get; private set; }

    // TODO: use up and down arrow keys to cycle through command history
    private static readonly List<string> commandHistory = [];
    private int commandHistoryIndex;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _EnterTree()
    {
        CommandInterpreter.Register(this);
    }

    public override void _ExitTree()
    {
        CommandInterpreter.Unregister(this);
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
        commandInput.TextSubmitted += OnCommandInputSubmit;
        Console = new InterpreterConsole(WriteOutput, WriteError);
    }

    private void WriteOutput(string text)
    {
        output.Text += text;
        Log.Info(text);
    }

    private void WriteError(string text)
    {
        WriteOutput($"[color=#ff0000]{text}[/color]");
        Log.Error(text);
    }

    private void OnCommandInputSubmit(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        var command = text.Split(" ").First();
        var args = string.Join(" ", text.Split(" ").Skip(1).Select(x => x.StartsWith('-') ? $"[color=#989898]{x}[/color]" : x));

        output.Text += $"> [color=#ffff00]{command}[/color] {args}\n";

        CommandInterpreter.Execute(text, Console);
        commandInput.Clear();
        commandInput.GrabFocus();

        commandHistory.Add(text);

        output.ScrollToLine(output.GetLineCount());
        commandHistoryIndex = commandHistory.Count;
    }

    public override void _Input(InputEvent @event)
    {
        if (commandHistory.Count == 0) return;

        if (@event.IsActionPressed("ui_up"))
        {
            commandHistoryIndex = Mathf.Clamp(commandHistoryIndex - 1, 0, commandHistory.Count - 1);
            commandInput.Text = commandHistory[commandHistoryIndex];
            commandInput.CaretColumn = commandInput.Text.Length;
            commandInput.GrabFocus();
        }
        else if (@event.IsActionPressed("ui_down"))
        {
            commandHistoryIndex = Mathf.Clamp(commandHistoryIndex + 1, 0, commandHistory.Count);
            commandInput.Text = commandHistoryIndex == commandHistory.Count ? string.Empty : commandHistory[commandHistoryIndex];
            commandInput.CaretColumn = commandInput.Text.Length;
            commandInput.GrabFocus();
        }
    }

    [Command(Name = "clear", Description = "Clears the console output.")]
    private void Clear() => output.Text = string.Empty;

    [Command(Name = "logging", Description = "Toggles logging.")]
    private void ToggleLogging(
        [CommandOption(Name = "--enable", Description = "Enables logging."), CommandOption(Name = "-e")]
        bool enable = false,
        [CommandOption(Name = "--disable", Description = "Disables logging."), CommandOption(Name = "-d")]
        bool disable = false
    )
    {
        if (enable && disable)
        {
            Console.Error.WriteLine("Cannot enable and disable logging at the same time.");
            return;
        }

        Log.SetEnabled(enable || !disable);
        Console.WriteLine($"Logging is now {(Log.Enabled ? "enabled" : "disabled")}.");
    }

    [Command(Name = "exit", Description = "Exits the game.")]
    private void Exit() => GetTree().Quit();

    [Command(Name = "history", Description = "Prints the command history.")]
    private void History(
        [CommandOption(Name = "--clear", Description = "Clears the command history."), CommandOption(Name = "-c")]
        bool clear = false
    )
    {
        if (clear)
        {
            commandHistory.Clear();
            commandHistoryIndex = 0;
            Console.WriteLine("Command history cleared.");
            return;
        }

        foreach (var command in commandHistory)
            Console.WriteLine(command);
    }
}