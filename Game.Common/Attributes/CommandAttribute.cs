namespace Game.Common.Utilities;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class CommandAttribute : Attribute
{
    public required string Name;
    public required string Description;
}