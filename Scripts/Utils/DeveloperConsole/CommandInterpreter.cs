using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Game.Globals;

public static class CommandInterpreter
{
    public delegate void CommandExecutedEventHandler(string command, object[] args);
    public delegate void CommandRegisteredEventHandler(string command, string description);
    public delegate void CommandUnregisteredEventHandler(string command);
    public delegate void HistoryChangedEventHandler();

    public static event CommandExecutedEventHandler CommandExecuted;
    public static event CommandRegisteredEventHandler CommandRegistered;
    public static event CommandUnregisteredEventHandler CommandUnregistered;
    public static event HistoryChangedEventHandler HistoryChanged;

    private const int MaxHistorySize = 100;
    private static readonly List<(string Command, DateTime Timestamp, Exception Exception)> history = [];
    private static readonly Dictionary<string, (Delegate Handler, string Description)> commands = [];

    public static IReadOnlyList<(string Command, DateTime Timestampm, Exception Exception)> History => history;


    public static void Register(string name, Delegate command, string description = null)
    {
        if (commands.ContainsKey(name))
        {
            throw new InvalidOperationException($"Command '{name}' already exists.");
        }

        commands[name] = (command, description);
        CommandRegistered?.Invoke(name, description);
    }

    public static void Unregister(string name)
    {
        if (!commands.ContainsKey(name))
        {
            throw new InvalidOperationException($"Command '{name}' does not exist.");
        }

        commands.Remove(name);
        CommandUnregistered?.Invoke(name);
    }

    public static async void Execute(string commandInput)
    {
        (string Command, DateTime Timestamp, Exception Exception) historyEntry = (commandInput, DateTime.Now, null);
        try
        {
            var commandParts = commandInput.Split([' '], StringSplitOptions.RemoveEmptyEntries);
            var commandName = commandParts.FirstOrDefault();

            if (string.IsNullOrEmpty(commandName) || !commands.TryGetValue(commandName, out var command))
            {
                throw new InvalidOperationException($"Command '{commandName}' does not exist.");
            }

            var (handler, args) = ParseCommand(commandInput, command.Handler);

            history.Add(historyEntry);
            TrimHistory();
            HistoryChanged?.Invoke();
            CommandExecuted?.Invoke(commandInput, args);

            await InvokeHandlerAsync(handler, args);
        }
        catch (Exception e)
        {
            historyEntry.Exception = e;
            history.Add(historyEntry);
            TrimHistory();
            HistoryChanged?.Invoke();
            CommandExecuted?.Invoke(commandInput, null);

            if (e is not FormatException and not InvalidOperationException)
                throw;
        }
    }

    public static string[] AutoComplete(string input)
    {
        var parts = input.Split([' '], StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 0
            ? []
            : [.. history
                .Where(entry => entry.Command.StartsWith(parts[0]))
                .Select(entry => entry.Command)
                .Distinct()
                ];
    }

    private static (Delegate Handler, object[] Args) ParseCommand(string commandInput, Delegate handler)
    {
        var parts = commandInput.Split([' '], StringSplitOptions.RemoveEmptyEntries);
        var parameters = handler.Method.GetParameters();

        if (parts.Length - 1 != parameters.Length)
        {
            throw new InvalidOperationException($"Command '{parts[0]}' expects {parameters.Length} arguments, but {parts.Length - 1} were provided.");
        }

        var args = parts.Skip(1)
            .Select((arg, i) => ParseArgument(arg, parameters[i].ParameterType))
            .ToArray();

        return (handler, args);
    }

    private static object ParseArgument(string argument, Type targetType)
    {
        if (targetType == typeof(string))
            return argument;

        try
        {
            // Try to find TryParse method
            var tryParseMethod = targetType.GetMethod("TryParse", [typeof(string), targetType.MakeByRefType()]);

            if (tryParseMethod != null)
            {
                var parameters = new object[] { argument, null };
                var success = (bool)tryParseMethod.Invoke(null, parameters);
                return success ? parameters[1] : throw new FormatException($"Failed to parse '{argument}' as {targetType.Name}.");
            }

            // Fallback to Parse method
            var parseMethod = targetType.GetMethod("Parse", [typeof(string)]);
            return parseMethod?.Invoke(null, [argument]) ?? throw new FormatException($"Failed to parse '{argument}' as {targetType.Name}.");
        }
        catch (Exception e)
        {
            throw new FormatException($"Failed to parse '{argument}' as {targetType.Name}.", e);
        }
    }

    private static async Task InvokeHandlerAsync(Delegate handler, object[] args)
    {
        var result = handler.DynamicInvoke(args);

        if (result is Task task)
        {
            await task;

            if (task.Exception == null) return;

            var exception = task.Exception.InnerException ?? task.Exception;

            if (exception is AggregateException aggregateException)
            {
                exception = aggregateException.InnerExceptions.Count == 1
                    ? aggregateException.InnerExceptions[0]
                    : aggregateException;
            }

            throw exception;
        }
    }

    private static void TrimHistory()
    {
        while (history.Count > MaxHistorySize)
            history.RemoveAt(0);
    }
}