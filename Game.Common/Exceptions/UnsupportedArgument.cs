using System;

namespace Game.Exceptions;

public class UnsupportedArgument : FormatException
{
    public UnsupportedArgument() : base("Unsupported arguments.") { }

    public UnsupportedArgument(string message) : base(message) { }

    public UnsupportedArgument(string message, Exception innerException) : base(message, innerException) { }
}