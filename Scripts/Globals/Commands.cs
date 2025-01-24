using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace Game.Globals;

// TODO: Add a way to connect to ui and not use print
public partial class Commands : Global<Commands>
{
    public delegate void CommandExecutedEventHandler(string command, object[] args);
    public delegate void CommandErrorEventHandler(Exception exception);
    public delegate void CommandNotFoundEventHandler(string command);
    public delegate void CommandRegisteredEventHandler(string command, string description);
    public delegate void CommandUnregisteredEventHandler(string command);
    public delegate void HistoryChangedEventHandler();

    public static event CommandExecutedEventHandler CommandExecuted;
    public static event CommandErrorEventHandler CommandError;
    public static event CommandNotFoundEventHandler CommandNotFound;
    public static event CommandRegisteredEventHandler CommandRegistered;
    public static event CommandUnregisteredEventHandler CommandUnregistered;
    public static event HistoryChangedEventHandler HistoryChanged;

    private const int MAX_HISTORY_SIZE = 100;
    private static readonly List<(string, DateTime)> commandHistory = [];

    private static readonly Dictionary<string, (Delegate, string)> commands = [];

    public override void _Ready()
    {
        Register("help", Help, description: "Prints a list of all available commands.");
        Register("quit", Quit, description: "Quits the game.");
        Register("history", History, description: "Prints the command history.");
        Register("clear", Clear, description: "Clears the console.");
    }

    public static void Register(string name, Delegate command, string description = null)
    {
        if (commands.ContainsKey(name))
        {
            ThrowException(new InvalidOperationException($"Command '{name}' already exists."));
            return;
        }

        commands[name] = (command, description);

        CommandRegistered?.Invoke(name, description);
    }

    public static void Unregister(string name)
    {
        if (!commands.ContainsKey(name))
            throw new KeyNotFoundException($"Command '{name}' not found.");

        commands.Remove(name);

        CommandUnregistered?.Invoke(name);
    }

    public static async void Execute(string command)
    {
        try
        {
            var name = command.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

            if (!commands.TryGetValue(name ?? "", out var value))
            {
                CommandNotFound?.Invoke(name);
                return;
            }

            var (deleg, args) = ParseCommand(command);

            commandHistory.Add((command, DateTime.Now));
            HistoryChanged?.Invoke();
            CommandExecuted?.Invoke(command, args);

            if (commandHistory.Count > MAX_HISTORY_SIZE)
                commandHistory.RemoveAt(0);

            var returnType = deleg.Method.ReturnType;

            if (returnType == typeof(Task) ||
                (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
               )
            {
                dynamic task = (Task)deleg.DynamicInvoke(args);

                if (task is null) return;

                await task;

                if (task.Exception != null)
                    throw task.Exception;

                return;
            }

            deleg.DynamicInvoke(args);
        }
        catch (Exception e)
        {
            ThrowException(e);
        }
    }

    private void Help()
    {
        // TODO: Print to UI instead of console

        GD.Print("Available commands:");

        foreach (var command in commands)
        {
            var (name, (deleg, description)) = command;

            var parameters = deleg.Method.GetParameters().Select(param => param.Name.ToString().ToLower()).ToArray();

            GD.Print($"{name} {string.Join(" ", parameters)} - {description}");
        }
    }

    private void Echo(string obj) => GD.Print(obj);

    private void Quit() => GetTree().Quit();

    private static void ThrowException(Exception exception)
    {
        CommandError?.Invoke(exception);
        throw exception;
    }

    private static (Delegate, object[]) ParseCommand(string command)
    {
        var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var name = parts[0];

        var (action, _) = commands[name];

        if (parts.Length - 1 != action.Method.GetParameters().Length)
            ThrowException(new InvalidOperationException(
                $"Command '{name}' expects {action.Method.GetParameters().Length} arguments, but {parts.Length - 1} were provided."));


        var args = parts.Skip(1).Select((arg, i) =>
        {
            var paramType = action.Method.GetParameters()[i].ParameterType;

            if (paramType == typeof(string))
                return arg;

            var parser = paramType.GetMethod("TryParse", [typeof(string), paramType.MakeByRefType()]) ??
                         paramType.GetMethod("Parse", [typeof(string)]);

            if (parser == null)
                ThrowException(
                    new InvalidOperationException(
                        $"No suitable parser found for argument '{arg}' of type '{paramType.Name}'.")
                    );

            return parser!.Invoke(null, [arg]);
        }).ToArray();

        return (action, args);
    }

    private static void History()
    {
        // TODO: Print to UI instead of console

        GD.Print("Command history:");

        foreach (var (command, time) in commandHistory)
        {
            GD.Print($"[{time}] - {command}");
        }

        if (commandHistory.Count == 0)
            GD.Print("No commands in history.");
    }

    private static void Clear()
    {
        // TODO: Clear UI
    }

    private static void ClearHistory()
    {
        commandHistory.Clear();
        HistoryChanged?.Invoke();
    }

    public static string[] AutoComplete(string command)
    {
        var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
            return [];

        var completes = commandHistory
            .Where((pair) => pair.Item1.StartsWith(parts[0]))
            .Select((pair) => pair.Item1)
            .ToArray();

        return completes;
    }
}