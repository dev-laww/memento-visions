using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.Linq;
using Game.Autoload;
using Game.Common;
using Game.Common.Extensions;
using Game.Common.Utilities;
using Game.Components;
using Game.Data;
using Game.Entities;
using Game.UI.Overlays;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game;

[Scene]
public partial class DeveloperConsole : Overlay
{
    [Node] LineEdit commandInput;
    [Node] RichTextLabel output;

    public static InterpreterConsole Console { get; private set; }

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
        var args = string.Join(
            " ",
            text.Split(" ").Skip(1).Select(x => x.StartsWith('-') ? $"[color=#989898]{x}[/color]" : x)
        );

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
            commandInput.Text = commandHistoryIndex == commandHistory.Count
                ? string.Empty
                : commandHistory[commandHistoryIndex];
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

    [Command(Name = "transition", Description = "Transitions to a scene.")]
    private void Transition(string scene)
    {
        if (string.IsNullOrWhiteSpace(scene))
        {
            Console.Error.WriteLine("Scene name cannot be empty.");
            return;
        }

        var isUsingGameManager = GetTree().Root.GetNodeOrNull("/root/GameManager") != null;


        if (!SceneRegistry.Get(scene, out var res))
        {
            Console.Error.WriteLine($"Scene '{scene}' not found.");
            return;
        }

        if (isUsingGameManager)
        {
            GameManager.ChangeScene(res);
        }
        else
        {
            SceneManager.ChangeScene(res);
        }
    }

    [Command(Name = "spawn", Description = "Spawns an entity at the given position.")]
    private void Spawn(
        string id,
        int amount = 1,
        [CommandOption(Name = "-x", Description = "X position")]
        float x = 0,
        [CommandOption(Name = "-y", Description = "Y position")]
        float y = 0
    )
    {
        if (id.Contains("player", System.StringComparison.CurrentCultureIgnoreCase) && this.GetPlayer() != null)
        {
            throw new System.Exception("Cannot spawn player entity when player already exists.");
        }

        var entity = EntityRegistry.Get(id) ?? throw new System.Exception($"Entity with id {id} not found.");

        var position = new Vector2(x, y);
        position = position == Vector2.Zero ? this.GetPlayer()?.GlobalPosition ?? Vector2.Zero : position;

        var isUsingGameManager = GetTree().Root.GetNodeOrNull("/root/GameManager") != null;

        for (var i = 0; i < amount; i++)
        {
            var instance = entity.Instantiate<Entity>();
            instance.GlobalPosition = position + (Vector2.One * MathUtil.RNG.RandfRange(-10, 10));

            if (isUsingGameManager)
            {
                GameManager.CurrentScene.AddChild(instance);
            }
            else
            {
                GetTree().CurrentScene.AddChild(instance);
            }
        }
    }

    [Command(Name = "telegraph", Description = "Creates a telegraph at the given position.")]
    private void Telegraph(
        [CommandOption(Name = "-x", Description = "X position")]
        float x = 0,
        [CommandOption(Name = "-y", Description = "Y position")]
        float y = 0
    )
    {
        var canvas = GetTree().Root.GetFirstChildOrNull<TelegraphCanvas>();

        if (canvas == null)
        {
            Console.Error.WriteLine("TelegraphCanvas not found.");
            return;
        }

        DamageBuilder.Circle()
            .WithRadius(140)
            .WithDamage(10)
            .WithDamage(1000)
            .WithWaitTime(3)
            .WithPosition(new Vector2(x, y))
            .WithOwner(this.GetPlayer())
            .Build();
    }
}