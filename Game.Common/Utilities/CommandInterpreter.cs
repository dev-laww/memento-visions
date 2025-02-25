using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Game.Common.Utilities;

public static class CommandInterpreter
{
    private static class Helper
    {
        public static Option? CreateOption(ParameterInfo parameter)
        {
            var attributes = parameter.GetCustomAttributes<CommandOptionAttribute>();

            var commandOptionAttributes = attributes as CommandOptionAttribute[] ?? [.. attributes];
            if (commandOptionAttributes.Length == 0) return null;

            var aliases = commandOptionAttributes.Select(attribute => attribute.Name).ToArray();
            var description = commandOptionAttributes.Select(attribute => attribute.Description).FirstOrDefault();

            var generic = typeof(Option<>).MakeGenericType(parameter.ParameterType);
            var instance = Activator.CreateInstance(generic, [aliases, description]);

            if (instance is not Option option) throw new InvalidOperationException("Failed to create option.");

            if (parameter.HasDefaultValue)
            {
                option.SetDefaultValue(parameter.DefaultValue);
            }

            return option;
        }

        public static Argument CreateArgument(ParameterInfo parameter)
        {
            var generic = typeof(Argument<>).MakeGenericType(parameter.ParameterType);
            var instance = Activator.CreateInstance(generic);

            if (instance is not Argument argument) throw new InvalidOperationException("Failed to create argument.");

            argument.Name = parameter.Name ?? string.Empty;
            argument.Description = parameter.Name;

            if (!parameter.HasDefaultValue) return argument;

            argument.Arity = ArgumentArity.ZeroOrOne;
            argument.SetDefaultValue(parameter.DefaultValue);

            return argument;
        }
    }

    private static Command rootCommand = Command;
    private static readonly List<Command> commands = [];
    // Temporary solution on command interpreter not working on MacOS/Linux
    private static Command Command => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new RootCommand("Game Commands") : new Command("game", "Game Commands");
    public static void Register(object obj)
    {
        var type = obj.GetType();

        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<CommandAttribute>();

            if (attribute == null) continue;

            RegisterCommand(attribute, method, obj);
        }
    }

    private static void RegisterCommand(CommandAttribute attribute, MethodInfo method, object target)
    {
        var command = new Command(attribute.Name, attribute.Description);

        var parameters = method.GetParameters();

        foreach (var parameter in parameters)
        {
            if (Helper.CreateOption(parameter) is { } option)
            {
                command.AddOption(option);
                continue;
            }

            var argument = Helper.CreateArgument(parameter);
            command.AddArgument(argument);
        }

        command.Handler = CommandHandler.Create(method, target); // how about async methods?

        commands.Add(command);
        rootCommand.AddCommand(command);
    }

    public static void Unregister(object obj)
    {
        var type = obj.GetType();

        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<CommandAttribute>();

            if (attribute == null) continue;

            commands.RemoveAll(command => command.Name == attribute.Name);
        }

        RebuildRootCommand();
    }

    private static void RebuildRootCommand()
    {
        rootCommand = Command;

        foreach (var command in commands)
        {
            rootCommand.AddCommand(command);
        }
    }

    public static void Execute(string command, IConsole? console = null)
    {
        rootCommand.Invoke(command, console);
    }
}