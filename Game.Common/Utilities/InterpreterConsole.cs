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

    public InterpreterConsole(IStandardStreamWriter outWriter, IStandardStreamWriter errorWriter)
    {
        Out = outWriter;
        Error = errorWriter;
    }

    public InterpreterConsole()
    {
        Out = InterpreterConsoleWriter.Create(Console.WriteLine);
        Error = InterpreterConsoleWriter.Create(Console.Error.WriteLine);
    }
}