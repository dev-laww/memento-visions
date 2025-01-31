using Game.Exceptions;

namespace Game.Common.Utilities;

public static class CommandInterpreter
{
    public delegate void CommandExecutedEventHandler(string command, object[] args);
    public delegate void CommandRegisteredEventHandler(string command, string? description);
    public delegate void CommandUnregisteredEventHandler(string command);

    public static event CommandExecutedEventHandler? CommandExecuted;
    public static event CommandRegisteredEventHandler? CommandRegistered;
    public static event CommandUnregisteredEventHandler? CommandUnregistered;

    private const int MaxHistorySize = 100;
    private static readonly List<(string Command, DateTime Timestamp, Exception Exception)> history = [];
    private static readonly Dictionary<string, (Delegate? Action, string? Description)> commands = [];

    public static IReadOnlyList<(string Command, DateTime Timestampm, Exception Exception)> History => history;
    public static IReadOnlyDictionary<string, (Delegate? Action, string? Description)> Commands => commands;


    public static void Register(string name, Delegate command, string? description = null)
    {
        if (commands.ContainsKey(name))
        {
            throw new InvalidOperationException($"Command '{name}' already exists.");
        }

        commands[name] = (command, description);
        CommandRegistered?.Invoke(name, description);
        Log.Debug($"Command '{name}' registered.");
    }

    public static void Unregister(string name)
    {
        if (!commands.ContainsKey(name))
        {
            throw new InvalidOperationException($"Command '{name}' does not exist.");
        }

        commands.Remove(name);
        CommandUnregistered?.Invoke(name);
        Log.Debug($"Command '{name}' unregistered.");
    }

    public static async void Execute(string commandInput)
    {
        (string Command, DateTime Timestamp, Exception Exception) historyEntry = (commandInput!, DateTime.Now!, null!);
        object[] args = [];

        try
        {
            var (name, action, parsedArgs) = ParseCommand(commandInput);
            args = parsedArgs;

            await InvokeHandlerAsync(action, args);

            if (!name.Equals("clear"))
            {
                history.Add(historyEntry);
                TrimHistory();
            }

            CommandExecuted?.Invoke(commandInput, args);
            Log.Debug($"Command '{name}' executed.");
        }
        catch (Exception e)
        {
            historyEntry.Exception = e.InnerException ?? e;
            history.Add(historyEntry);
            TrimHistory();
            CommandExecuted?.Invoke(commandInput, args);

            if (historyEntry.Exception is not CommandException)
            {
                Log.Error(historyEntry.Exception);
                throw;
            }
        }
    }

    public static string[] AutoComplete(string input)
    {
        var parts = input.Split([' '], StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 0
            ? []
            :
            [
                .. history
                    .Where(entry => entry.Command.StartsWith(parts[0]))
                    .Select(entry => entry.Command)
                    .Distinct()
            ];
    }

    private static (string Name, Delegate action, object[] args) ParseCommand(string input)
    {
        var name = commands.Keys.OrderByDescending(k => k.Length).FirstOrDefault(input.StartsWith) ?? input;

        if (string.IsNullOrEmpty(name) || !commands.TryGetValue(name, out var command))
            throw new CommandException($"Command '{name}' does not exist.");

        var parts = input.Replace(name, string.Empty).Split([' '], StringSplitOptions.RemoveEmptyEntries);

        var action = command.Action;
        var parameters = action!.Method.GetParameters();

        var args = new object[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            if (i >= parts.Length)
            {
                if (!parameters[i].HasDefaultValue)
                    throw new CommandException($"Missing argument '{parameters[i].Name}'.");

                args[i] = parameters[i].DefaultValue ?? throw new InvalidOperationException();
                continue;
            }

            args[i] = ParseArgument(parts[i].Trim(), parameters[i].ParameterType);
        }

        return (name, action, args);
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
                var parameters = new object[] { argument, null! };
                var success = (bool?)tryParseMethod.Invoke(null, parameters) ?? false;
                return success
                    ? parameters[1]
                    : throw new UnsupportedArgument($"Failed to parse '{argument}' as {targetType.Name}.");
            }

            // Fallback to Parse method
            var parseMethod = targetType.GetMethod("Parse", [typeof(string)]);
            return parseMethod?.Invoke(null, [argument]) ??
                   throw new UnsupportedArgument($"Failed to parse '{argument}' as {targetType.Name}.");
        }
        catch (Exception e)
        {
            Log.Error(e);
            throw new UnsupportedArgument($"Failed to parse '{argument}' as {targetType.Name}.", e);
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

    public static void ClearHistory() => history.Clear();
}