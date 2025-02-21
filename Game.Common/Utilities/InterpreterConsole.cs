using System.CommandLine;
using System.CommandLine.IO;

namespace Game.Common.Utilities;


public class InterpreterConsole : IConsole
{
    public IStandardStreamWriter Out { get; }
    public IStandardStreamWriter Error { get; }

    public bool IsOutputRedirected => false;

    public bool IsErrorRedirected => false;

    public bool IsInputRedirected => false;

    public InterpreterConsole(Action<string?> outWriter, Action<string?> errorWriter)
    {
        Out = InterpreterConsoleWriter.Create(outWriter);
        Error = InterpreterConsoleWriter.Create(errorWriter);
    }

    public InterpreterConsole()
    {
        Out = InterpreterConsoleWriter.Create(Console.WriteLine);
        Error = InterpreterConsoleWriter.Create(Console.Error.WriteLine);
    }
}