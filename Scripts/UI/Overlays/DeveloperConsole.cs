using System.Collections.Generic;
using System.CommandLine;
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
    private readonly List<string> commandHistory = [];

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
        output.ScrollToLine(output.GetLineCount());

        CommandInterpreter.Execute(text, Console);
        commandInput.Clear();
        commandInput.GrabFocus();

        commandHistory.Add(text);
    }

    public override void _Input(InputEvent @event)
    {
        if (!@event.IsActionPressed("ui_up") || commandHistory.Count == 0) return;

        commandInput.Text = commandHistory[^1];
        commandInput.GrabFocus();
    }

    [Command(Name = "clear", Description = "Clears the console output.")]
    private void Clear() => output.Text = string.Empty;

}