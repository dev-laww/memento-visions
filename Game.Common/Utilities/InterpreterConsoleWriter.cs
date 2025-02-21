using System.CommandLine.IO;

namespace Game.Common.Utilities;

public class InterpreterConsoleWriter(Action<string?> action) : IStandardStreamWriter
{
    public void Write(string? value) => action(value);

    public static InterpreterConsoleWriter Create(Action<string?> action) => new(action);
}
