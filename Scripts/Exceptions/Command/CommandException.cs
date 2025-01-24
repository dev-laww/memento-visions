using System;

namespace Game.Exceptions.Command;

public class CommandException(string message) : Exception(message);